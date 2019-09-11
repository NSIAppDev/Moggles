using System;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Moggles.Tests
{
    public class TestStartup : Startup
    {
        public TestStartup(IConfiguration config) : base(config)
        {
            config["Messaging:UseMessaging"] = "true";
        }

        public override void ConfigureDatabaseServices(IServiceCollection services)
        {
            // Do nothing
        }

        public override void ConfigureAuthServices(IServiceCollection services)
        {
            services.AddAuthorization(options => { options.AddPolicy("OnlyAdmins", policy => policy.RequireAssertion(ctx => true)); });
        }

        public override IBusControl ConfigureMessageBus(IServiceProvider serviceProvider)
        {
            return Bus.Factory.CreateUsingInMemory(sbc =>
            {
                sbc.ReceiveEndpoint("test_queue", e =>
                {
                    e.LoadFrom(serviceProvider);
                });
            });
        }
    }
}
