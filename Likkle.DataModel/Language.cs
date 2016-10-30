using System;
using System.Collections.Generic;

namespace Likkle.DataModel
{
    public class Language
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        // Navigation properties
        public virtual ICollection<User> Users { get; set; } 
    }
}
