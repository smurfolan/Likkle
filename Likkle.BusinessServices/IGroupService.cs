using System;
using System.Collections.Generic;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessEntities.Responses;

namespace Likkle.BusinessServices
{
    public interface IGroupService
    {
        IEnumerable<GroupMetadataResponseDto> GetGroups(double latitude, double longitude);
        IEnumerable<GroupMetadataResponseDto> AllGroups();
        Guid InsertNewGroup(StandaloneGroupRequestDto newGroup);
        Guid InserGroupAsNewArea(GroupAsNewAreaRequestDto newGroup);
        GroupDto GetGroupById(Guid groupId);
        IEnumerable<UserDto> GetUsersFromGroup(Guid groupId);
        IEnumerable<Guid> GetUserSubscriptions(Guid uid, double lat, double lon);
    }
}
