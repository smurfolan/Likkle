using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Likkle.BusinessEntities.Enums;

namespace Likkle.BusinessEntities.Requests
{
    public class GroupAsNewAreaRequestDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public IEnumerable<Guid> TagIds { get; set; }

        public IEnumerable<Guid> AreaIds { get; set; }
        public Guid UserId { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        [EnumDataType(typeof(RadiusRangeEnum))]
        public RadiusRangeEnum Radius { get; set; }
    }
}
