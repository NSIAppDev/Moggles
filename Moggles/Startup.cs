using System;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moggles.BackgroundServices;
using Moggles.Consumers;
using Moggles.Data.SQL;
using Moggles.Data.NoDb;
using NoDb;
using Moggles.Domain;
using Microsoft.AspNetCore.Http;

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
            ConfigureAuthServices(services);

            services.AddMvc();

            ConfigureDatabaseServices(services);

            if (bool.TryParse(Configuration.GetSection("Messaging")["UseMessaging"], out bool useMassTransitAndMessaging) && useMassTransitAndMessaging)
            {
                ConfigureMassTransitAndMessageBus(services);
            }

            services.AddNoDb<Application>();
            services.AddNoDb<ToggleSchedule>();
            services.AddScoped<IRepository<Application>, ApplicationsRepository>();
            services.AddScoped<IRepository<ToggleSchedule>, ToggleSchedulesRepository>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddHostedService<ScheduledFeatureTogglesService>();
        }

        public virtual void ConfigureDatabaseServices(IServiceCollection services)
        {
            services.AddDbContext<TogglesContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("FeatureTogglesConnection")));
        }

        public virtual void ConfigureAuthServices(IServiceCollection services)
        {
            var admins = Configuration.GetSection("CustomRoles")["Admins"];

            services.AddAuthentication(IISDefaults.AuthenticationScheme);
            services.AddAuthorization(options => { options.AddPolicy("OnlyAdmins", policy => policy.RequireRole(admins)); });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, Microsoft.AspNetCore.Hosting.IApplicationLifetime appLifetime, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
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
                //app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
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
                var host = sbc.Host(new Uri(Configuration.GetSection("Messaging")["Url"]), h =>
                {
                    h.Username(Configuration.GetSection("Messaging")["Username"]);
                    h.Password(Configuration.GetSection("Messaging")["Password"]);
                });

                sbc.ReceiveEndpoint(host, Configuration.GetSection("Messaging")["QueueName"], e =>
                {
                    e.LoadFrom(serviceProvider);
                });
            });
        }
    }
}
