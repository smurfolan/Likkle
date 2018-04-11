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
            return base.OnReconnected();
        }

        /// <summary>
        /// Executes at the end of a SignalR connection. A SignalR connection can end in any of the following ways:
        /// - If the client calls the Stop method, a stop message is sent to the server, and both client and server end the SignalR connection immediately.
        /// - After connectivity between client and server is lost, the client tries to reconnect and the server waits for the client to reconnect. If the attempts to reconnect are unsuccessful and the disconnect timeout period ends, both client and server end the SignalR connection.
        /// - If the client stops running without having a chance to call the Stop method, the server waits for the client to reconnect, and then ends the SignalR connection after the disconnect timeout period.
        /// - If the server stops running, the client tries to reconnect (re-create the transport connection), and then ends the SignalR connection after the disconnect timeout period.
        /// </summary>
        /// <param name="stopCalled">true if the client explicitly closed the connection</param>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            // NOTE: We have 4 cases in which a connection can be stopped. 
            // In only one of them (client calls the Stop method) we could actually determine who is the user and remove him from Groups
            if (stopCalled)
            {
                var userId = Context.QueryString[UserIdQueryStringParamName];
                if (!string.IsNullOrEmpty(userId))
                {
                    Groups.Remove(Context.ConnectionId, userId);
                }
            }
            
            return base.OnDisconnected(stopCalled);
        }
    }
}
