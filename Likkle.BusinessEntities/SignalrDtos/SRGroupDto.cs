using System;
using System.Collections.Generic;

namespace Likkle.BusinessEntities.SignalrDtos
{
    public class SRGroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<Guid> UserIds { get; set; }
        public IEnumerable<Guid> TagIds { get; set; }
    }
}
