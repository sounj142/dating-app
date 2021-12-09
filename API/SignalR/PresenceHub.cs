using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly IPresenceTracker _presenceTracker;

        public PresenceHub(IPresenceTracker presenceTracker)
        {
            _presenceTracker = presenceTracker;
        }

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User.GetUserName();
            _presenceTracker.AddConnection(userName, Context.ConnectionId);

            if (_presenceTracker.TotalConnections(userName) == 1)
            {
                await Clients.Others.SendAsync("UserIsOnline", userName);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userName = Context.User.GetUserName();
            _presenceTracker.RemoveConnection(userName, Context.ConnectionId);

            if (!_presenceTracker.IsOnline(userName))
            {
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUserName());
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
