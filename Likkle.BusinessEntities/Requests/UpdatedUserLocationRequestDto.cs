using System.ComponentModel.DataAnnotations;

namespace Likkle.BusinessEntities.Requests
{
    public class UpdatedUserLocationRequestDto
    {
        [Required]
        public double LatestLatitude { get; set; }

        [Required]
        public double LatestLongitude { get; set; } 
    }
}
