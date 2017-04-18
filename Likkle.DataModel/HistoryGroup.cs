using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Likkle.DataModel
{
    public class HistoryGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public DateTime DateTimeGroupWasSubscribed { get; set; }

        public Guid UserId { get; set; }
        [Required]
        public virtual User UserWhoSubscribedGroup { get; set; }

        public Guid GroupId { get; set; }
        [Required]
        public virtual Group GroupThatWasPreviouslySubscribed { get; set; }
    }
}
