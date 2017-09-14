using System.Security.Principal;
using Likkle.BusinessEntities;

namespace Likkle.BusinessServices
{
    public interface ISubscriptionService
    {
        void RelateUserToGroups(RelateUserToGroupsDto newRelations);

        void UpdateLatestWellKnownUserLocation(double latitude, double longitude, IPrincipal user);
    }
}
