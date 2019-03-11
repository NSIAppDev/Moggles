using MogglesContracts;

namespace MogglesClient.Messaging
{
    public interface IMogglesBusService
    {
        void ConfigureAndStartMessageBus();
        void Publish(RegisteredTogglesUpdate registeredTogglesUpdate);
    }
}
