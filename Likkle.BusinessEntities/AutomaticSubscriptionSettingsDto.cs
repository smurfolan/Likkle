using System;
using System.Collections.Generic;

namespace Likkle.BusinessEntities
{
    public class AutomaticSubscriptionSettingsDto
    {
        public Guid Id { get; set; }

        public bool AutomaticallySubscribeToAllGroups { get; set; }

        public bool AutomaticallySubscribeToAllGroupsWithTag { get; set; }

        public IEnumerable<Guid> SubscribedTagIds { get; set; }
    }
}
