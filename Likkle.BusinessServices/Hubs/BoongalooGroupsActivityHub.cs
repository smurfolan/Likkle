using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace Likkle.BusinessServices.Hubs
{
    public class BoongalooGroupsActivityHub : Hub
    {
        private readonly string UserIdQueryStringParamName = "userId";
        private readonly string CurrentlyConnectedUsersGroupName = "CurrentlyConnectedUsers";
        public override Task OnConnected()
        {
            // It has to be explicitly passed by the client when creating the HubConnection. 
            // In the .NET client it is as a second parameter - dictionary.
            var userId = Context.QueryString[UserIdQueryStringParamName];
            Groups.Add(userId, CurrentlyConnectedUsersGroupName);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var userId = Context.QueryString[UserIdQueryStringParamName];
            Groups.Remove(userId, CurrentlyConnectedUsersGroupName);

            return base.OnDisconnected(stopCalled);
        }

        #region Client RPCs

        #endregion

        #region Server-side RPCs
        #endregion
    }
}
