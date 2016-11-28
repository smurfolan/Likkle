using System;
using System.Collections.Generic;

namespace Likkle.BusinessEntities.Requests
{
    public class MultipleAreasMetadataRequestDto
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public IEnumerable<Guid> AreaIds { get; set; }
    }
}
