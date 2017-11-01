using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Likkle.BusinessEntities.Requests
{
    public class BaseNewGroupRequest
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public IEnumerable<Guid> TagIds { get; set; }

        [Required]
        public bool VisibleToThePublic { get; set; }

        public Guid UserId { get; set; }
    }
}
