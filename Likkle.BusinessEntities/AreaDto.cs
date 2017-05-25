using System;
using System.Collections.Generic;
using Likkle.BusinessEntities.Enums;

namespace Likkle.BusinessEntities
{
    public class AreaDto
    {
        public Guid Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ApproximateAddress { get; set; }

        public RadiusRangeEnum Radius { get; set; }

        public IEnumerable<GroupDto> Groups { get; set; } 
    }
}
