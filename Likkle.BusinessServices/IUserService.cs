using System;
using System.Collections.Generic;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessEntities.Responses;

namespace Likkle.BusinessServices
{
    public interface IUserService
    {
        UserInfoResponseDto GetUserById(Guid userId);
        UserInfoResponseDto GetUserByStsId(string stsId);

        IEnumerable<UserDto> GetAllUsers();
        Guid InsertNewUser(NewUserRequestDto newUser);
        void UpdateUserInfo(Guid uid, UpdateUserInfoRequestDto updatedInfo);
        void UpdateUserNotificationSettings(Guid uid, EditUserNotificationsRequestDto edittedUserNotificationSettings);
        NotificationSettingDto GetNotificationSettingsForUserWithId(Guid uid);
        void UpdateUserLocation(Guid id, double lat, double lon);
        SocialLinksResponseDto GetSocialLinksForUser(Guid userId);
        void UpdateSocialLinksForUser(Guid userId, UpdateSocialLinksRequestDto updatedSocialLinks);
    }
}
