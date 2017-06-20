using System;
using System.Collections.Generic;

namespace Likkle.BusinessServices
{
    public interface ISubscriptionSettingsService
    {
        IEnumerable<Guid> GroupsForUserAroundCoordinatesBasedOnUserSettings(
            Guid userId, 
            double latitude,
            double longitude);
    }
}
