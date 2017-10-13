using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace Likkle.BusinessServices.Hubs
{
    public class BoongalooGroupsActivityHub : Hub
    {
        private readonly string UserIdQueryStringParamName = "userId";

        public override Task OnConnected()
        {
            // It has to be explicitly passed by the client when creating the HubConnection. 
            // In the .NET client it is as a second parameter - dictionary.
            var userId = Context.QueryString[UserIdQueryStringParamName];

            // If one day we have clients from different platforms they would be coming with different connection id but same user id.
            // When grouping by user id we can easily broadcast to all the platforms this user is currently in.
            Groups.Add(Context.ConnectionId, userId);

            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            var userId = Context.QueryString[UserIdQueryStringParamName];
            Groups.Add(Context.ConnectionId, userId);

            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var userId = Context.QueryString[UserIdQueryStringParamName];
            Groups.Remove(Context.ConnectionId, userId);

            return base.OnDisconnected(stopCalled);
        }
    }
}
