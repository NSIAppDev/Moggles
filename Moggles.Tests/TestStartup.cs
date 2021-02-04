using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Moggles.Tests
{
    public class TestStartup : Startup
    {
        public TestStartup(IConfiguration config) : base(config)
        {
            config["Messaging:UseMessaging"] = "true";
        }

        public override void ConfigureAuthServices(IServiceCollection services)
        {
            services.AddAuthorization(options => { options.AddPolicy("OnlyAdmins", policy => policy.RequireAssertion(ctx => true)); });
        }

        public override IBusControl ConfigureMessageBus(IServiceProvider serviceProvider)
        {
            var busControl =  Bus.Factory.CreateUsingInMemory(sbc =>
            {
                sbc.ReceiveEndpoint("test_queue", e =>
                {
                    e.LoadFrom(serviceProvider);
                });
            });

            return busControl;
        }
    }
}
