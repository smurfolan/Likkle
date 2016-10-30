﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Likkle.DataModel
{
    public class Group
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Name { get; set; }

        // Navigation properties
        public virtual ICollection<Area> Areas { get; set; } 

        public virtual ICollection<Tag> Tags { get; set; } 
    }
}
