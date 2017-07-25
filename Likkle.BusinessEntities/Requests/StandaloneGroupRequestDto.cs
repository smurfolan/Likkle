using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Likkle.BusinessEntities.Requests
{
    public class StandaloneGroupRequestDto : BaseNewGroupRequest
    {
        [Required]
        public IEnumerable<Guid> AreaIds { get; set; }
    }
}
