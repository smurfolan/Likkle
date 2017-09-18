using Likkle.BusinessEntities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Likkle.BusinessEntities.SignalrDtos
{
    public class SRAreaDto
    {
        public Guid Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ApproximateAddress { get; set; }
        public RadiusRangeEnum Radius { get; set; }
        public IEnumerable<Guid> GroupIds { get; set; }
    }
}
