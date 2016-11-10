using System.ComponentModel.DataAnnotations;
using Likkle.BusinessEntities.Enums;

namespace Likkle.BusinessEntities.Requests
{
    public class NewAreaRequest
    {
        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        public RadiusRangeEnum Radius { get; set; }
    }
}
