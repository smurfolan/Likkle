using System;
using System.Collections.Generic;
using Likkle.BusinessEntities.Enums;

namespace Likkle.BusinessEntities.Requests
{
    public class UpdateUserInfoRequestDto
    {
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string Email { get; set; }

        public string About { get; set; }
        public GenderEnum Gender { get; set; }

        public DateTime BirthDate { get; set; }
        public string PhoneNumber { get; set; }

        public IEnumerable<Guid> LanguageIds { get; set; }
    }
}
