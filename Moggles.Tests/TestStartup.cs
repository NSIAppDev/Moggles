using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moggles.Consumers;
using System;

namespace Moggles.Tests
{
    public class TestStartup : Startup
    {
        public TestStartup(IConfiguration config) : base(config)
        {
            config["Messaging:UseMessaging"] = "true";
            config["EnableEntraId"] = "false";
		}

		public override void ConfigureAuthServices(IServiceCollection services)
		{
			// Add a test authentication scheme
			services.AddAuthentication("Test")
				.AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>(
					"Test", options => { });

			services.AddAuthorization(options =>
			{
				options.AddPolicy("OnlyAdmins", policy => policy.RequireAssertion(ctx => true));
			});
		}

		public override IBusControl ConfigureMessageBus(IServiceProvider serviceProvider)
        {
            var busControl =  Bus.Factory.CreateUsingInMemory(sbc =>
            {
                sbc.ReceiveEndpoint("test_queue", e =>
                {
                    e.Consumer<FeatureToggleDeployStatusConsumer>(serviceProvider);
                });
            });

            return busControl;
        }
    }
}
