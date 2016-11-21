using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Likkle.BusinessEntities.Enums;

namespace Likkle.BusinessEntities.Requests
{
    public class NewUserRequestDto
    {
        [Required]
        public string IdsrvUniqueId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        public string About { get; set; }
        public GenderEnum Gender { get; set; }

        public DateTime? BirthDate { get; set; }
        public string PhoneNumber { get; set; }

        public IEnumerable<Guid> LanguageIds { get; set; }
        public IEnumerable<Guid> GroupIds { get; set; }
    }
}
