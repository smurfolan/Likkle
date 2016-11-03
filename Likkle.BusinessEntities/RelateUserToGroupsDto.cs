using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Likkle.BusinessEntities
{
    public class RelateUserToGroupsDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public IEnumerable<Guid> GroupsUserSubscribes { get; set; }
    }
}
