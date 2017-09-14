using System.Security.Principal;
using Likkle.BusinessEntities;
using System.Collections.Generic;
using System;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessEntities.Enums;

namespace Likkle.BusinessServices
{
    public interface ISubscriptionService
    {
        void RelateUserToGroups(RelateUserToGroupsDto newRelations);
        void UpdateLatestWellKnownUserLocation(double latitude, double longitude, IPrincipal user);
        void AutoSubscribeUsersFromExistingAreas(IEnumerable<Guid> areaIds, StandaloneGroupRequestDto newGroupMetadata, Guid newGroupId);
        void AutoSubscribeUsersForGroupAsNewArea(double newAreaLat, double newAreaLon, RadiusRangeEnum newAreaRadius, Guid newGroupId);
    }
}
