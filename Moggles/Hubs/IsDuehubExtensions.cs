using Microsoft.AspNetCore.SignalR;
using Moggles.Domain;

namespace Moggles.Hubs
{
    public static class IsDuehubExtensions
    {
        public static void NotifyClient(this IHubContext<IsDueHub, IIsDueHub> hub,ToggleSchedule toggle)
        {
            hub.Clients.All.IsDue(toggle);
        }
    }
}
