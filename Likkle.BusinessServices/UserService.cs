﻿using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using AutoMapper;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessEntities.Responses;
using Likkle.DataModel;
using Likkle.DataModel.UnitOfWork;

namespace Likkle.BusinessServices
{
    public class UserService : IUserService
    {
        private readonly string InitialDateString = "01/01/1753";

        private readonly ILikkleUoW _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfigurationWrapper _configuration;

        public UserService(
            ILikkleUoW uow,
            IConfigurationProvider configurationProvider,
            IConfigurationWrapper config)
        {
            this._unitOfWork = uow;
            _mapper = configurationProvider.CreateMapper();
            this._configuration = config;
        }

        public IEnumerable<UserDto> GetAllUsers()
        {
            var allUserEntities = this._unitOfWork.UserRepository.GetAllUsers();

            var userDtos = this._mapper.Map<IEnumerable<User>, IEnumerable<UserDto>>(allUserEntities);

            return userDtos;
        }

        public Guid InsertNewUser(NewUserRequestDto newUser)
        {
            var userEntity = this._mapper.Map<NewUserRequestDto, User>(newUser);
            userEntity.Id = Guid.NewGuid();

            if (newUser.BirthDate != null)
            {
                var userBirthdate = DateTime.Parse(newUser.BirthDate.ToString());
                var minDateForSql = DateTime.Parse(InitialDateString);

                userEntity.BirthDate = userBirthdate <= minDateForSql ? minDateForSql : newUser.BirthDate;
            }
            else
            {
                userEntity.BirthDate = DateTime.Parse(InitialDateString);
            }

            // add groups
            if (newUser.GroupIds != null && newUser.GroupIds.Any())
            {
                userEntity.Groups = new List<Group>();
                var groupEntities =
                    this._unitOfWork.GroupRepository.GetGroups().Where(gr => newUser.GroupIds.Contains(gr.Id));
                foreach (var groupEntity in groupEntities)
                {
                    userEntity.Groups.Add(groupEntity);
                }
            }

            // add languages
            if (newUser.LanguageIds != null && newUser.LanguageIds.Any())
            {
                userEntity.Languages = new List<Language>();
                var languageEntities =
                    this._unitOfWork.LanguageRepository.GetAlLanguages().Where(l => newUser.LanguageIds.Contains(l.Id));
                foreach (var languageEntity in languageEntities)
                {
                    userEntity.Languages.Add(languageEntity);
                }
            }

            // add default notification settings
            var newNotificationSettingEntity = new NotificationSetting()
            {
                AutomaticallySubscribeToAllGroupsWithTag = false,
                AutomaticallySubscribeToAllGroups = false
            };

            userEntity.NotificationSettings = newNotificationSettingEntity;

            this._unitOfWork.UserRepository.InsertNewUser(userEntity);
            this._unitOfWork.Save();

            return userEntity.Id;
        }

        public void UpdateUserInfo(Guid uid, UpdateUserInfoRequestDto updatedInfo)
        {
            var userEntity = this._unitOfWork.UserRepository.GetAllUsers().FirstOrDefault(u => u.Id == uid);

            if (userEntity == null)
                throw new ArgumentException("User not available in Database");

            this._mapper.Map<UpdateUserInfoRequestDto, User>(updatedInfo, userEntity);

            if (userEntity.Languages != null)
                userEntity.Languages.Clear();
            else
                userEntity.Languages = new List<Language>();

            var languages =
                this._unitOfWork.LanguageRepository.GetAlLanguages().Where(l => updatedInfo.LanguageIds.Contains(l.Id));

            foreach (var language in languages)
            {
                userEntity.Languages.Add(language);
            }

            this._unitOfWork.Save();
        }

        public void UpdateUserNotificationSettings(Guid uid, EditUserNotificationsRequestDto edittedUserNotificationSettings)
        {
            var userNotificationSettings = this._unitOfWork.UserRepository.GetUserById(uid).NotificationSettings;

            if (userNotificationSettings == null)
                throw new ArgumentException("There's no notification settings for the user");

            userNotificationSettings.AutomaticallySubscribeToAllGroups =
                edittedUserNotificationSettings.AutomaticallySubscribeToAllGroups;

            userNotificationSettings.AutomaticallySubscribeToAllGroupsWithTag =
                edittedUserNotificationSettings.AutomaticallySubscribeToAllGroupsWithTag;

            if (userNotificationSettings.Tags != null)
                userNotificationSettings.Tags.Clear();
            else
                userNotificationSettings.Tags = new List<Tag>();

            var tagsForNotification = this._unitOfWork.TagRepository.GetAllTags()
                .Where(t => edittedUserNotificationSettings.SubscribedTagIds.Contains(t.Id));

            var forNotification = tagsForNotification as Tag[] ?? tagsForNotification.ToArray();

            if (edittedUserNotificationSettings.SubscribedTagIds != null
                && edittedUserNotificationSettings.SubscribedTagIds.Any()
                && (forNotification.Count() != edittedUserNotificationSettings.SubscribedTagIds.Count()))
                throw new ArgumentException("Some of the tags that were submitted are not available in our system.");

            foreach (var tag in forNotification)
            {
                userNotificationSettings.Tags.Add(tag);
            }

            this._unitOfWork.Save();
        }

        public NotificationSettingDto GetNotificationSettingsForUserWithId(Guid uid)
        {
            var notificationEntity = this._unitOfWork.UserRepository.GetUserById(uid).NotificationSettings;

            var notificationSettingDto =
                this._mapper.Map<NotificationSetting, NotificationSettingDto>(notificationEntity);

            if (notificationEntity.Tags != null)
                notificationSettingDto.SubscribedTagIds = notificationEntity.Tags.Select(t => t.Id);

            return notificationSettingDto;
        }

        public SocialLinksResponseDto GetSocialLinksForUser(Guid userId)
        {
            var user = this._unitOfWork.UserRepository.GetUserById(userId);

            var socialLinks = new SocialLinksResponseDto()
            {
                FacebookUsername = user.FacebookUsername,
                InstagramUsername = user.InstagramUsername
            };

            return socialLinks;
        }

        public void UpdateSocialLinksForUser(Guid userId, UpdateSocialLinksRequestDto updatedSocialLinks)
        {
            var user = this._unitOfWork.UserRepository.GetUserById(userId);

            user.FacebookUsername = updatedSocialLinks.FacebookUsername;
            user.InstagramUsername = updatedSocialLinks.InstagramUsername;

            this._unitOfWork.Save();
        }

        /// <summary>
        /// Currently method is only used to detach users from groups in order to know keep only relevant information when showing up a group.
        /// </summary>
        /// <param name="id">Id of the user</param>
        /// <param name="lat">Latest latitude of the user</param>
        /// <param name="lon">Latest longitude of the user</param>
        public void UpdateUserLocation(Guid id, double lat, double lon)
        {
            var user = this._unitOfWork.UserRepository.GetUserById(id);
            var groupsTheUserIsCurrentlyIn = user.Groups.ToArray();

            foreach (var group in groupsTheUserIsCurrentlyIn)
            {
                if (!GroupIsAvailableAroundCoordinates(group, lat, lon))
                    user.Groups.Remove(group);
            }

            this._unitOfWork.Save();
        }

        public UserInfoResponseDto GetUserById(Guid userId)
        {
            var user = this._unitOfWork.UserRepository.GetUserById(userId);

            var userDto = this._mapper.Map<User, UserInfoResponseDto>(user);

            userDto.NotificationSettings = this.GetNotificationSettingsForUserWithId(userId);

            return userDto;
        }

        public UserInfoResponseDto GetUserByStsId(string stsId)
        {
            var user = this._unitOfWork.UserRepository.GetUserByStsId(stsId);

            var userDto = this._mapper.Map<User, UserInfoResponseDto>(user);

            return userDto;
        }

        #region Private methods
        private static bool GroupIsAvailableAroundCoordinates(Group group, double lat, double lon)
        {
            var areasOfTheGroup = group.Areas.ToList();

            var currentUserLocation = new GeoCoordinate(lat, lon);

            return
                areasOfTheGroup.Any(
                    area =>
                        currentUserLocation.GetDistanceTo(new GeoCoordinate(area.Latitude, area.Longitude)) <=
                        (int)area.Radius);
        }
        #endregion
    }
}