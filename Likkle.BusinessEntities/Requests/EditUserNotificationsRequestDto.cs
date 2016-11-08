using System;
using System.Collections.Generic;

namespace Likkle.BusinessEntities.Requests
{
    public class EditUserNotificationsRequestDto
    {
        public bool AutomaticallySubscribeToAllGroups { get; set; }

        public bool AutomaticallySubscribeToAllGroupsWithTag { get; set; }

        public IEnumerable<Guid> SubscribedTagIds;
    }
}
