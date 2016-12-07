using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using AutoMapper;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessEntities.Responses;
using Likkle.DataModel;
using Likkle.DataModel.UnitOfWork;

namespace Likkle.BusinessServices
{
    public class DataService : IDataService
    {
        private readonly string InitialDateString = "01/01/1753";

        private readonly LikkleUoW _unitOfWork;
        private readonly IMapper _mapper;

        public DataService()
        {
            _unitOfWork = new LikkleUoW();
            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.AddProfile<EntitiesMappingProfile>();
            });
            _mapper = mapperConfiguration.CreateMapper();
        }

        #region Area specific
        public IEnumerable<AreaDto> GetAllAreas()
        {
            var allAreaEntities = this._unitOfWork.AreaRepository.GetAreas();

            var areasAsDtos = this._mapper.Map<IEnumerable<Area>, IEnumerable<AreaDto>>(allAreaEntities);

            return areasAsDtos;
        }

        public AreaDto GetAreaById(Guid areaId)
        {
            var area = this._unitOfWork.AreaRepository.GetAreaById(areaId);

            var areaAsDto = this._mapper.Map<Area, AreaDto>(area);

            return areaAsDto;
        }

        public IEnumerable<AreaDto> GetAreasForGroupId(Guid groupId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AreaForLocationResponseDto> GetAreas(double latitude, double longitude)
        {
            var currentLocation = new GeoCoordinate(latitude, longitude);

            var areaEntities = this._unitOfWork.AreaRepository.GetAreas()
                .Where(x => currentLocation.GetDistanceTo(new GeoCoordinate(x.Latitude, x.Longitude)) <= (int)x.Radius);

            var areasAsDtos = this._mapper.Map<IEnumerable<Area>, IEnumerable<AreaForLocationResponseDto>>(areaEntities);

            return areasAsDtos;
        }

        public IEnumerable<AreaForLocationResponseDto> GetAreas(double lat, double lon, int rad)
        {
            var currentLocation = new GeoCoordinate(lat, lon);

            var areaEntities = this._unitOfWork.AreaRepository.GetAreas()
                .Where(a => currentLocation.GetDistanceTo(new GeoCoordinate(a.Latitude, a.Longitude)) <= rad);

            var areasAsDtos = this._mapper.Map<IEnumerable<Area>, IEnumerable<AreaForLocationResponseDto>>(areaEntities);

            return areasAsDtos;
        }

        public IEnumerable<UserDto> GetUsersFromArea(Guid areaId)
        {
            var users = this._unitOfWork.AreaRepository.GetAreas()
                .Where(a => a.Id == areaId)
                .SelectMany(ar => ar.Groups.SelectMany(gr => gr.Users));

            var usersAsDtos = this._mapper.Map<IEnumerable<User>, IEnumerable<UserDto>>(users);

            return usersAsDtos;
        }

        public AreaMetadataResponseDto GetMetadataForArea(
            double latitude, 
            double longitude, 
            Guid areaId)
        {
            var clickedArea = this.GetAreaById(areaId);
            var clientLocation = new GeoCoordinate(latitude, longitude);
            var areaCenterLocation = new GeoCoordinate(clickedArea.Latitude, clickedArea.Longitude);

            return GetAreaMetadata(clientLocation, areaCenterLocation, clickedArea);
        }

        public IEnumerable<AreaMetadataResponseDto> GetMultipleAreasMetadata(MultipleAreasMetadataRequestDto areas)
        {
            var areaEntities = this._unitOfWork.AreaRepository.GetAreas().Where(a => areas.AreaIds.Contains(a.Id));

            var areaDtos = this._mapper.Map<IEnumerable<Area>, IEnumerable<AreaDto>>(areaEntities);

            var areaEntitiesAsMetadataList =
                areaDtos.Select(
                    a =>
                        GetAreaMetadata(new GeoCoordinate(areas.Latitude, areas.Longitude),
                            new GeoCoordinate(a.Latitude, a.Longitude), a));

            return areaEntitiesAsMetadataList;
        }

        public Guid InsertNewArea(NewAreaRequest newArea)
        {
            var areaEntity = this._mapper.Map<NewAreaRequest, Area>(newArea);
            
            return this._unitOfWork.AreaRepository.Insert(areaEntity);
        }

        #endregion

        #region Group specific

        public IEnumerable<GroupMetadataResponseDto> GetGroups(double latitude, double longitude)
        {
            var result = this._unitOfWork.AreaRepository.GetAreas(latitude, longitude);

            var groupsInsideAreas = result.SelectMany(ar => ar.Groups).Distinct();

            var groupsAsDtos = this._mapper.Map<IEnumerable<Group>, IEnumerable<GroupMetadataResponseDto>>(groupsInsideAreas);

            return groupsAsDtos;
        }

        // TODO: Extract common parts from InsertNewGroup and InserGroupAsNewArea
        public Guid InsertNewGroup(StandaloneGroupRequestDto newGroup)
        {
            var newGroupEntity = this._mapper.Map<StandaloneGroupRequestDto, Group>(newGroup);

            newGroupEntity.Areas = new List<Area>();
            newGroupEntity.Tags = new List<Tag>();
            newGroupEntity.Users = new List<User>();

            if (newGroup.UserId != Guid.Empty)
            {
                var user = this._unitOfWork.UserRepository.GetUserById(newGroup.UserId);

                if(user != null)
                    newGroupEntity.Users.Add(user);
            }     

            if (newGroup.TagIds != null && newGroup.TagIds.Any())
            {
                var tagsForGroup = this._unitOfWork.TagRepository.GetAllTags().Where(t => newGroup.TagIds.Contains(t.Id));
                foreach (var tag in tagsForGroup)
                {
                    newGroupEntity.Tags.Add(tag);
                }
            }

            if (newGroup.AreaIds != null && newGroup.AreaIds.Any())
            {
                var areasForGroup = this._unitOfWork.AreaRepository.GetAreas().Where(a => newGroup.AreaIds.Contains(a.Id));
                foreach (var area in areasForGroup)
                {
                    newGroupEntity.Areas.Add(area);
                }
            }           

            this._unitOfWork.GroupRepository.InsertGroup(newGroupEntity);

            this._unitOfWork.GroupRepository.Save();

            return newGroupEntity.Id;
        }

        public Guid InserGroupAsNewArea(GroupAsNewAreaRequestDto newGroup)
        {
            // 1. Add new area
            var areaEntity = new Area()
            {
                Latitude = newGroup.Latitude,
                Longitude = newGroup.Longitude,
                Radius = newGroup.Radius
            };

            var newAreaId = this._unitOfWork.AreaRepository.InsertArea(areaEntity);

            this._unitOfWork.AreaRepository.Save();

            // 2. Add new group
            var newGroupEntity = this._mapper.Map<GroupAsNewAreaRequestDto, Group>(newGroup);

            newGroupEntity.Tags = new List<Tag>();
            newGroupEntity.Areas = new List<Area>();
            newGroupEntity.Users = new List<User>();

            if (newGroup.UserId != Guid.Empty)
            {
                var user = this._unitOfWork.UserRepository.GetUserById(newGroup.UserId);

                if(user != null)
                    newGroupEntity.Users.Add(user);
            }           

            var newlyCreatedArea = this._unitOfWork.AreaRepository.GetAreaById(newAreaId);

            newGroupEntity.Areas.Add(newlyCreatedArea);

            if (newGroup.TagIds != null && newGroup.TagIds.Any())
            {
                var tagsForGroup = this._unitOfWork.TagRepository.GetAllTags().Where(t => newGroup.TagIds.Contains(t.Id));
                foreach (var tag in tagsForGroup)
                {
                    newGroupEntity.Tags.Add(tag);
                }
            }

            this._unitOfWork.GroupRepository.InsertGroup(newGroupEntity);

            this._unitOfWork.GroupRepository.Save();

            return newGroupEntity.Id;
        }

        public GroupDto GetGroupById(Guid groupId)
        {
            var group = this._unitOfWork.GroupRepository.GetGroupById(groupId);

            var groupDto = this._mapper.Map<Group, GroupDto>(group);

            return groupDto;
        }

        public IEnumerable<UserDto> GetUsersFromGroup(Guid groupId)
        {
            var users = this._unitOfWork.GroupRepository.GetGroupById(groupId).Users;

            var userDtos = this._mapper.Map<IEnumerable<User>, IEnumerable<UserDto>>(users);

            return userDtos;
        }



        #endregion

        #region User specific

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
        // TODO: To be seriously unit tested
        public void RelateUserToGroups(RelateUserToGroupsDto newRelations)
        {
            var user = this._unitOfWork.UserRepository.GetUserById(newRelations.UserId);

            if(user == null)
                throw new ArgumentException("User with id:" + newRelations.UserId + " doesn't exist.");

            if(user.Groups == null)
                user.Groups = new List<Group>();

            var currentLocation = new GeoCoordinate(newRelations.Latitude, newRelations.Longitude);

            var areaEntities = this._unitOfWork.AreaRepository.GetAreas()
                .Where(x => currentLocation.GetDistanceTo(new GeoCoordinate(x.Latitude, x.Longitude)) <= (int)x.Radius);

            // 1. Fetch all the groups that belong to the user and are around {newRelations.Latitude, newRelations.Longitude}
            var groupsForUserAroundCurrentLocation = areaEntities
                .SelectMany(a => a.Groups)
                .Where(gr => gr.Users.Contains(user));

            // 2. Fetch all the groups that came from the request
            var groupSubscriptionsThatCameFromTheRequest =
                this._unitOfWork.GroupRepository.GetGroups()
                    .Where(gr => newRelations.GroupsUserSubscribes.Contains(gr.Id));

            // 3. Determine what groups have been unsubscribed: Everything that is in (1) and doesn't belong to (2)
            var forUserAroundCurrentLocation = groupsForUserAroundCurrentLocation as Group[] ?? groupsForUserAroundCurrentLocation.ToArray();

            var unsubscribedGroups =
                forUserAroundCurrentLocation.Where(gr => !groupSubscriptionsThatCameFromTheRequest.Contains(gr));

            // 4. Determine what groups have bee added and are new: Everything that is in (2) and doesn't belong to (1)
            var newlySubscribedGroups = groupSubscriptionsThatCameFromTheRequest.Where(gr => !forUserAroundCurrentLocation.Contains(gr));

            // 5. Each group in (3) has to be removed from current user subscriptions
            foreach (var unsubscribedGroup in unsubscribedGroups)
            {
                user.Groups.Remove(unsubscribedGroup);
            }

            // 6. Each group in (4) has to be added to the current user subscriptions
            foreach (var newlySubscribedGroup in newlySubscribedGroups)
            {
                user.Groups.Add(newlySubscribedGroup);
            }

            this._unitOfWork.Save();
        }

        public IEnumerable<UserDto> GetAllUsers()
        {
            var allUserEntities = this._unitOfWork.UserRepository.GetAllUsers();

            var userDtos = this._mapper.Map<IEnumerable<User>, IEnumerable<UserDto>>(allUserEntities);

            return userDtos;
        }
        // TODO: To be seriously unit tested
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
            
            if(userEntity == null)
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

        public IEnumerable<Guid> GetUserSubscriptions(Guid uid, double lat, double lon)
        {
            var groupsAroundCoordinates = this.GetGroups(lat, lon).Select(gr => gr.Id);

            var allGroupsForUser = this._unitOfWork.UserRepository.GetUserById(uid).Groups.Select(gr => gr.Id);

            return allGroupsForUser.Where(gr => groupsAroundCoordinates.Contains(gr));
        }

        public void UpdateUserNotificationSettings(Guid uid, EditUserNotificationsRequestDto edittedUserNotificationSettings)
        {
            var userNotificationSettings = this._unitOfWork.UserRepository.GetUserById(uid).NotificationSettings;

            if(userNotificationSettings == null)
                throw new ArgumentException("There's no notification settings for the user");

            userNotificationSettings.AutomaticallySubscribeToAllGroups =
                edittedUserNotificationSettings.AutomaticallySubscribeToAllGroups;

            userNotificationSettings.AutomaticallySubscribeToAllGroupsWithTag =
                edittedUserNotificationSettings.AutomaticallySubscribeToAllGroupsWithTag;

            userNotificationSettings.Tags.Clear();

            var tagsForNotification = this._unitOfWork.TagRepository.GetAllTags()
                .Where(t => edittedUserNotificationSettings.SubscribedTagIds.Contains(t.Id));

            var forNotification = tagsForNotification as Tag[] ?? tagsForNotification.ToArray();

            if(edittedUserNotificationSettings.SubscribedTagIds != null 
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

            notificationSettingDto.SubscribedTagIds = notificationEntity.Tags.Select(t => t.Id);

            return notificationSettingDto;
        }

        #endregion

        #region Private methods
        private static AreaMetadataResponseDto GetAreaMetadata(
            GeoCoordinate clientLocation,
            GeoCoordinate areaCenterLocation,
            AreaDto clickedArea)
        {
            // 1. Get the distance between lat/lon and area's center
            var pointToAreaCenterDistance = clientLocation.GetDistanceTo(areaCenterLocation);

            var distance = pointToAreaCenterDistance > (double)clickedArea.Radius
                ? pointToAreaCenterDistance - (double)clickedArea.Radius
                : pointToAreaCenterDistance;

            // 2. Get total number of people in the area (all groups people)
            var totalNumberOfParticipants = clickedArea.Groups.SelectMany(gr => gr.Users).Count();

            // 3. Gather all the uniqe group tags from all the groups
            var allTags = clickedArea.Groups.SelectMany(gr => gr.Tags).Select(t => t.Id).Distinct();

            return new AreaMetadataResponseDto()
            {
                DistanceTo = distance,
                NumberOfParticipants = totalNumberOfParticipants,
                TagIds = allTags
            };
        }
        #endregion
    }
}
