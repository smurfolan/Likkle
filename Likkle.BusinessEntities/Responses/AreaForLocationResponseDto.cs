using System;
using Likkle.BusinessEntities.Enums;

namespace Likkle.BusinessEntities.Responses
{
    public class AreaForLocationResponseDto
    {
        public Guid Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ApproximateAddress { get; set; }

        public RadiusRangeEnum Radius { get; set; }
    }
}
