using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Likkle.BusinessEntities.Requests
{
    public class BaseNewGroupRequest
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public IEnumerable<Guid> TagIds { get; set; }

        public Guid UserId { get; set; }
    }
}
