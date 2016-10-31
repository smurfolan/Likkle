using Likkle.BusinessEntities.Enums;

namespace Likkle.BusinessEntities.Requests
{
    public class NewAreaRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public RadiusRangeEnum Radius { get; set; }
    }
}
