using Microsoft.AspNetCore.SignalR;
using Moggles.Domain;
using System.Threading.Tasks;

namespace Moggles.Hubs
{

    public interface IIsDueHub
    {
        Task IsDue(ToggleSchedule toggleSchedule);
    }

    public class IsDueHub : Hub<IIsDueHub>
    { 
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
