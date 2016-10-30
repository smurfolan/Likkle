using System;
using System.Collections.Generic;

namespace Likkle.BusinessEntities
{
    public class AreaDto
    {
        public Guid Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public IEnumerable<GroupDto> Groups { get; set; } 
    }
}
