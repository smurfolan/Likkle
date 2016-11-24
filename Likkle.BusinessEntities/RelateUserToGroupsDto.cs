using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Likkle.BusinessEntities
{
    public class RelateUserToGroupsDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public IEnumerable<Guid> GroupsUserSubscribes { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }
    }
}
