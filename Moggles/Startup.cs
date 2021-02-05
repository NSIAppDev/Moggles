using System;
using System.IO;
using System.Text;
using GreenPipes;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moggles.BackgroundServices;
using Moggles.Consumers;
using Moggles.Data.NoDb;
using NoDb;
using Moggles.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.IdentityModel.Tokens;
using Moggles.Hubs;

namespace Moggles
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            ConfigureAuthServices(services);

            services.AddSignalR(o =>
            {
                //The recommended value is double the KeepAliveInterval value, as per documentation
                o.ClientTimeoutInterval = TimeSpan.FromSeconds(120);

                //The recommended value for KeepAliveInterval is half the serverTimeoutInMilliseconds on the client
                o.KeepAliveInterval = TimeSpan.FromSeconds(60);

                o.EnableDetailedErrors = true;
                o.HandshakeTimeout = TimeSpan.FromSeconds(20);
            });


            services.AddApplicationInsightsTelemetry();

            if (bool.TryParse(Configuration["Messaging:UseMessaging"], out bool useMassTransitAndMessaging) && useMassTransitAndMessaging)
            {
                ConfigureMassTransitAndMessageBus(services);
            }

            services.AddNoDb<Application>();
            services.AddNoDb<ToggleSchedule>();
            services.AddScoped<IRepository<Application>, ApplicationsRepository>();
            services.AddScoped<IRepository<ToggleSchedule>, ToggleSchedulesRepository>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddHostedService<ScheduledFeatureTogglesService>();

            services.AddMvc(options =>
            {
                options.Conventions.Add(new AuthorizationPolicyConvention("OnlyAdmins", Configuration.UseJwt(), JwtBearerDefaults.AuthenticationScheme));
            });
        }

        public virtual void ConfigureAuthServices(IServiceCollection services)
        {
            var admins = Configuration["CustomRoles:Admins"];

            services.AddAuthentication(IISDefaults.AuthenticationScheme);

            RegisterJwtAuthentication(services);
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy("OnlyAdmins", policy => policy.RequireRole(admins));
            });
        }


        private void RegisterJwtAuthentication(IServiceCollection services)
        {
            var tokenSigningKey = Configuration.GetTokenSigningKey();

            if (string.IsNullOrEmpty(tokenSigningKey))
                return;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSigningKey))
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

#pragma warning disable CS0618 // Type or member is obsolete
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    HotModuleReplacement = true
                });
            }
            else if (env.IsStaging())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapFallbackToController("Index", "Home");
                endpoints.MapHub<IsDueHub>("/isDueHub", options => {
                    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
                });
                
            });
        }

        private void ConfigureMassTransitAndMessageBus(IServiceCollection services)
        {
            services.AddMassTransit(c => { c.AddConsumer<FeatureToggleDeployStatusConsumer>(); });

            services.AddSingleton(ConfigureMessageBus);

            services.AddSingleton<IPublishEndpoint>(provider => provider.GetRequiredService<IBusControl>());
            services.AddSingleton<ISendEndpointProvider>(provider => provider.GetRequiredService<IBusControl>());
            services.AddSingleton<IBus>(provider => provider.GetRequiredService<IBusControl>());

            services.AddSingleton<IHostedService, BusService>();
        }

        public virtual IBusControl ConfigureMessageBus(IServiceProvider serviceProvider)
        {
            return Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var host = sbc.Host(new Uri(Configuration["Messaging:Url"]), h =>
                {
                    h.Username(Configuration["Messaging:Username"]);
                    h.Password(Configuration["Messaging:Password"]);
                });

                sbc.UseRetry(retryCfg =>
                {
                    retryCfg.Handle<IOException>();

                    retryCfg.Interval(10, TimeSpan.FromMinutes(1));
                });

                sbc.ReceiveEndpoint(host, Configuration["Messaging:QueueName"], e =>
                {
                    e.Consumer<FeatureToggleDeployStatusConsumer>(serviceProvider);
                    e.PrefetchCount = 1;
                });
            });
        }
    }
}
