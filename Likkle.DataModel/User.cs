using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Likkle.BusinessEntities.Enums;

namespace Likkle.DataModel
{
    public class User
    {     
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string IdsrvUniqueId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public string About { get; set; }
        public GenderEnum Gender { get; set; }

        public DateTime? BirthDate { get; set; }
        public string PhoneNumber { get; set; }

        public string InstagramUsername { get; set; }
        public string FacebookUsername { get; set; }
        public string TwitterUsername { get; set; }

        // Navigation properties
        public virtual ICollection<Group> Groups { get; set; } 

        public virtual ICollection<Language> Languages { get; set; }

        public virtual NotificationSetting NotificationSettings { get; set; }

        public virtual ICollection<HistoryGroup> HistoryGroups { get; set; }      
    }
}
