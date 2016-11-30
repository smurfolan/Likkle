using System;
using System.Collections.Generic;

namespace Likkle.BusinessEntities.Responses
{
    public class GroupMetadataResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public int NumberOfUsers { get; set; }
        public IEnumerable<Guid> TagIds { get; set; }
    }
}
