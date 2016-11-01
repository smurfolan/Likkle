using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Likkle.BusinessEntities.Requests
{
    public class StandaloneGroupRequestDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public IEnumerable<Guid> TagIds { get; set; }

        [Required]
        public IEnumerable<Guid> AreaIds { get; set; }

        public Guid UserId { get; set; }
    }
}
