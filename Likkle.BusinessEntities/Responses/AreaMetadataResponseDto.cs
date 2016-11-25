using System;
using System.Collections.Generic;

namespace Likkle.BusinessEntities.Responses
{
    public class AreaMetadataResponseDto
    {
        // Distance from the client to the area
        public double DistanceTo { get; set; }

        // What is the total number of all the people currently inside the area
        public int NumberOfParticipants { get; set; }

        // What are the people talking about in this area
        public IEnumerable<Guid> TagIds { get; set; }
    }
}
