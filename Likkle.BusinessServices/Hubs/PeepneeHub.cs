using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace Likkle.BusinessServices.Hubs
{
    public class PeepneeHub : Hub
    {
        public override Task OnConnected()
        {
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
            return base.OnDisconnected(stopCalled);
        }

        #region Mobile device defined endpoints
        public void NewMailRequest(string takenPictureUrl, string ocrParsedText)
        {
            // newMailRequest to be defined on the device
            Clients.All.newMailRequest(takenPictureUrl, ocrParsedText);
        }
        #endregion

        #region Raspberry pi defined endpoints
        public void MailRequestAccepted()
        {
            // mailRequestAccepted to be defined on the Raspberry
            Clients.All.mailRequestAccepted();
        }

        public void MailRequestDeclined()
        {
            // mailRequestDeclined to be defined on the Raspberry
            Clients.All.mailRequestDeclined();
        }

        public void RepeatMailRequest()
        {
            // repeatMailRequest to be defined on the Raspberry
            Clients.All.repeatMailRequest();
        }

        public void UpdateDefaultOwnerSettings(bool openAfterDefaultTime, int secondsToDefaultBehaviour)
        {
            // updateDefaultOwnerSettings to be defined on the Raspberry
            Clients.All.updateDefaultOwnerSettings(openAfterDefaultTime, secondsToDefaultBehaviour);
        }
        #endregion
    }
}
