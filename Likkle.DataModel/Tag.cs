using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Likkle.DataModel
{
    public class Tag
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; }

        // Navigation properties
        public virtual ICollection<Group> Groups { get; set; }
        public virtual ICollection<AutomaticSubscriptionSetting> AutomaticSubscriptionSettings { get; set; }
    }
}
