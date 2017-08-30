using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace Likkle.BusinessServices.Hubs
{
    public class BoongalooGroupsActivityHub : Hub
    {
        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled);
        }

        #region Client RPCs
        #endregion

        #region Server-side RPCs
        #endregion
    }
}
