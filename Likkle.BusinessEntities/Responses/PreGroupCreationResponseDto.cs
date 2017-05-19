using System.Collections.Generic;
using Likkle.BusinessEntities.Enums;

namespace Likkle.BusinessEntities.Responses
{
    public class PreGroupCreationResponseDto
    {
        public CreateGroupActionTypeEnum CreationType { get; set; }
        public IEnumerable<RecreateGroupRecord> PrevousGroupsList { get; set; }
    }
}
