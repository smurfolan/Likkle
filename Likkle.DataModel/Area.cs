using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Likkle.BusinessEntities.Enums;

namespace Likkle.DataModel
{
    public class Area
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public double Latitude { get; set; }
        
        public double Longitude { get; set; }

        public bool IsActive { get; set; }

        public string ApproximateAddress { get; set; }

        public RadiusRangeEnum Radius { get; set; }

        // Navigation properties
        public virtual ICollection<Group> Groups { get; set; } 
    }
}
