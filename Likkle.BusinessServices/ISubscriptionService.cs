﻿using Likkle.BusinessEntities;

namespace Likkle.BusinessServices
{
    public interface ISubscriptionService
    {
        void RelateUserToGroups(RelateUserToGroupsDto newRelations);

        void UpdateLatestWellKnownUserLocation(decimal latitude, decimal longitude);
    }
}
