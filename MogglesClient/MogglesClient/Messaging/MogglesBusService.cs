using System;
using MassTransit;
using MogglesClient.Messaging.RefreshCache;
using MogglesClient.PublicInterface;
using MogglesContracts;

namespace MogglesClient.Messaging
{
    public class MogglesBusService : IMogglesBusService
    {
        private readonly IMogglesConfigurationManager _mogglesConfigurationManager;
        private IBusControl _busControl;

        public MogglesBusService(IMogglesConfigurationManager mogglesConfigurationManager)
        {
            _mogglesConfigurationManager = mogglesConfigurationManager;
        }

        public void ConfigureAndStartMessageBus()
        {
            _busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(new Uri(_mogglesConfigurationManager.GetMessageBusUrl()), h =>
                {
                    h.Username(_mogglesConfigurationManager.GetMessageBusUser());
                    h.Password(_mogglesConfigurationManager.GetMessageBusPassword());
                });

                cfg.ReceiveEndpoint(host, $"{_mogglesConfigurationManager.GetCacheRefreshQueue()}_{_mogglesConfigurationManager.GetApplicationName()}_{_mogglesConfigurationManager.GetEnvironment()}", e =>
                {
                    e.Consumer<ClearTogglesCacheConsumer>();
                });

            });

            _busControl.Start();
        }

        public void Publish(RegisteredTogglesUpdate registeredTogglesUpdate)
        {
            _busControl.Publish(registeredTogglesUpdate);
        }
    }
}
