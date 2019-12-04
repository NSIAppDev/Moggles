using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moggles.Domain;
using System.Threading.Tasks;

namespace Moggles.Consumers
{
    public class IsDueHub : Hub
    {
        public async Task SendMessage(ToggleSchedule toggleSchedule, string message)
        {
             await Clients.All.SendAsync("ReceiveMessage", toggleSchedule, message);
        }

        public override Task OnConnectedAsync()
        {
            string name = Context.User.Identity.Name ?? Context.UserIdentifier;
            if (name != null)
            {
                Groups.AddToGroupAsync(Context.ConnectionId, name);
            }
            return base.OnConnectedAsync();
        }
    }
}
