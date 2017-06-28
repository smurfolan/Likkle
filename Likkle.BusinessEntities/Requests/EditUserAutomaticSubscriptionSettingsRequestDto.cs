using System;
using System.Collections.Generic;

namespace Likkle.BusinessEntities.Requests
{
    public class EditUserAutomaticSubscriptionSettingsRequestDto
    {
        public bool AutomaticallySubscribeToAllGroups { get; set; }

        public bool AutomaticallySubscribeToAllGroupsWithTag { get; set; }

        public IEnumerable<Guid> SubscribedTagIds;
    }
}
