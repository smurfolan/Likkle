using System;
using System.Collections.Generic;
using Likkle.BusinessEntities.Enums;

namespace Likkle.BusinessEntities
{
    public class UserDto
    {
        public Guid Id { get; set; }

        public string IdsrvUniqueId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public string About { get; set; }
        public GenderEnum Gender { get; set; }

        public DateTime BirthDate { get; set; }
        public string PhoneNumber { get; set; }

        public virtual IEnumerable<GroupDto> Groups { get; set; }
        public virtual IEnumerable<LanguageDto> Languages { get; set; }
        public virtual NotificationSettingDto NotificationSettings { get; set; }
    }
}
