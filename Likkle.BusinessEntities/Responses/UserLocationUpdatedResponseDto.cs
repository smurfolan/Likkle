using System;
using System.Collections.Generic;

namespace Likkle.BusinessEntities.Responses
{
    public class UserLocationUpdatedResponseDto
    {
        public double SecodsToClosestBoundary { get; set; }
        public IEnumerable<Guid> SubscribedGroupIds { get; set; }
    }
}
