using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Likkle.BusinessEntities.Enums;

namespace Likkle.BusinessEntities.Requests
{
    public class GroupAsNewAreaRequestDto : BaseNewGroupRequest
    {
        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        [EnumDataType(typeof(RadiusRangeEnum))]
        public RadiusRangeEnum Radius { get; set; }
    }
}
