using System;
using System.Collections.Generic;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessEntities.Responses;

namespace Likkle.BusinessServices
{
    public interface IDataService
    {
        #region Area specific
        IEnumerable<AreaDto> GetAllAreas();
        AreaDto GetAreaById(Guid areaId);
        IEnumerable<AreaForLocationResponseDto> GetAreas(double latitude, double longitude);
        IEnumerable<UserDto> GetUsersFromArea(Guid areaId);
        AreaMetadataResponseDto GetMetadataForArea(double latitude, double longitude, Guid areaId);
        IEnumerable<AreaMetadataResponseDto> GetMultipleAreasMetadata(MultipleAreasMetadataRequestDto areas);
        IEnumerable<AreaForLocationResponseDto> GetAreas(double lat, double lon, int rad);

        Guid InsertNewArea(NewAreaRequest newArea);

        #endregion

        #region Group specific

        IEnumerable<GroupMetadataResponseDto> GetGroups(double latitude, double longitude);
        IEnumerable<GroupMetadataResponseDto> AllGroups();
        Guid InsertNewGroup(StandaloneGroupRequestDto newGroup);
        Guid InserGroupAsNewArea(GroupAsNewAreaRequestDto newGroup);
        GroupDto GetGroupById(Guid groupId);
        IEnumerable<UserDto> GetUsersFromGroup(Guid groupId);

        #endregion

        #region User specific

        UserInfoResponseDto GetUserById(Guid userId);
        UserInfoResponseDto GetUserByStsId(string stsId);
        void RelateUserToGroups(RelateUserToGroupsDto newRelations);
        IEnumerable<UserDto> GetAllUsers();
        Guid InsertNewUser(NewUserRequestDto newUser);
        void UpdateUserInfo(Guid uid, UpdateUserInfoRequestDto updatedInfo);
        IEnumerable<Guid> GetUserSubscriptions(Guid uid, double lat, double lon);
        void UpdateUserNotificationSettings(Guid uid, EditUserNotificationsRequestDto edittedUserNotificationSettings);
        NotificationSettingDto GetNotificationSettingsForUserWithId(Guid uid);
        #endregion
    }
}
