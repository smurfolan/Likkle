using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Likkle.DataModel
{
    public class AutomaticSubscriptionSetting
    {       
        [Key]
        //For the User - AutomaticSubscriptionSetting relation see: http://stackoverflow.com/questions/40506036/how-to-achieve-one-to-one-in-code-first-approach-if-we-have-auto-generated-ids/40515309#40515309
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ForeignKey("User")]
        public Guid Id { get; set; }
        public bool AutomaticallySubscribeToAllGroups { get; set; }
        public bool AutomaticallySubscribeToAllGroupsWithTag { get; set; }

        // Navigation properties
        [Required]
        public virtual User User { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }
    }
}
