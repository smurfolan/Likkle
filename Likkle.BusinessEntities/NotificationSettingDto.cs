using System;
using System.Collections.Generic;

namespace Likkle.BusinessEntities
{
    public class NotificationSettingDto
    {
        public Guid Id { get; set; }

        public bool AutomaticallySubscribeToAllGroups { get; set; }

        public bool AutomaticallySubscribeToAllGroupsWithTag { get; set; }

        public IEnumerable<int> SubscribedTagIds { get; set; }
    }
}
