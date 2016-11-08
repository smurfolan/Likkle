using System;
using System.Collections.Generic;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Requests;

namespace Likkle.BusinessServices
{
    public interface IDataService
    {
        #region Area specific
        IEnumerable<AreaDto> GetAllAreas();
        AreaDto GetAreaById(Guid areaId);
        IEnumerable<AreaDto> GetAreasForGroupId(Guid groupId);
        IEnumerable<AreaDto> GetAreas(double latitude, double longitude);
        IEnumerable<UserDto> GetUsersFromArea(Guid areaId);

        Guid InsertNewArea(NewAreaRequest newArea);

        #endregion

        #region Group specific

        IEnumerable<GroupDto> GetGroups(double latitude, double longitude);
        Guid InsertNewGroup(StandaloneGroupRequestDto newGroup);
        Guid InserGroupAsNewArea(GroupAsNewAreaRequestDto newGroup);
        GroupDto GetGroupById(Guid groupId);
        IEnumerable<UserDto> GetUsersFromGroup(Guid groupId);

        #endregion

        #region User specific

        UserDto GetUserById(Guid userId);
        UserDto GetUserByStsId(string stsId);
        void RelateUserToGroups(RelateUserToGroupsDto newRelations);
        IEnumerable<UserDto> GetAllUsers();
        Guid InsertNewUser(NewUserRequestDto newUser);
        void UpdateUserInfo(Guid uid, UpdateUserInfoRequestDto updatedInfo);
        IEnumerable<Guid> GetUserSubscriptions(Guid uid);
        void UpdateUserNotificationSettings(Guid uid, EditUserNotificationsRequestDto edittedUserNotificationSettings);
        NotificationSettingDto GetNotificationSettingsForUserWithId(Guid uid);

        #endregion
    }
}
