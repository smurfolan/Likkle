using System;
using System.Collections.Generic;

namespace Likkle.BusinessEntities
{
    public class GroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool VisibleToThePublic { get; set; }
        public IEnumerable<UserDto> Users { get; set; }
        public IEnumerable<TagDto> Tags { get; set; }
    }
}
