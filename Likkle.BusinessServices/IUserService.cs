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
        void UpdateUserAutomaticSubscriptionSettings(Guid uid, EditUserAutomaticSubscriptionSettingsRequestDto edittedUserNotificationSettings);
        AutomaticSubscriptionSettingsDto GetAutomaticSubscriptionSettingsForUserWithId(Guid uid);
        UserLocationUpdatedResponseDto UpdateUserLocation(Guid id, double lat, double lon);
        SocialLinksResponseDto GetSocialLinksForUser(Guid userId);
        void UpdateSocialLinksForUser(Guid userId, UpdateSocialLinksRequestDto updatedSocialLinks);
    }
}
