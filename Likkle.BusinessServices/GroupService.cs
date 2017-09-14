using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using AutoMapper;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Enums;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessEntities.Responses;
using Likkle.BusinessServices.Utils;
using Likkle.DataModel;
using Likkle.DataModel.UnitOfWork;

namespace Likkle.BusinessServices
{
    public class GroupService : IGroupService
    {
        private readonly string GroupRecreateUrlTemplate = @"api/v1/groups/{0}/Activate";

        private readonly ILikkleUoW _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfigurationWrapper _configuration;
        private readonly IGeoCodingManager _geoCodingManager;
        private readonly ISubscriptionService _subscriptionService;

        public GroupService(
            ILikkleUoW uow,
            IConfigurationProvider configurationProvider,
            IConfigurationWrapper config, 
            IGeoCodingManager geoCodingManager,
            ISubscriptionService subscriptionService)
        {
            this._unitOfWork = uow;
            _mapper = configurationProvider.CreateMapper();
            this._configuration = config;
            _geoCodingManager = geoCodingManager;
            _subscriptionService = subscriptionService;
        }

        public IEnumerable<GroupMetadataResponseDto> GetGroups(double latitude, double longitude)
        {
            var result = this._unitOfWork.AreaRepository.GetAreas(latitude, longitude);

            var groupsInsideAreas = result
                .SelectMany(ar => ar.Groups)
                .Distinct()
                .Where(gr => gr.IsActive);

            var groupsAsDtos = this._mapper.Map<IEnumerable<Group>, IEnumerable<GroupMetadataResponseDto>>(groupsInsideAreas);

            return groupsAsDtos;
        }

        public Guid InsertNewGroup(StandaloneGroupRequestDto newGroup)
        {
            var newGroupEntity = this._mapper.Map<StandaloneGroupRequestDto, Group>(newGroup);

            this.AssignBaseGroupInfo(newGroup, newGroupEntity);

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

            this._subscriptionService.AutoSubscribeUsersFromExistingAreas(newGroup.AreaIds, newGroup, newGroupEntity.Id);

            return newGroupEntity.Id;
        }
        
        public Guid InserGroupAsNewArea(GroupAsNewAreaRequestDto newGroup)
        {
            // 1. Add new area
            var areaEntity = new Area()
            {
                Latitude = newGroup.Latitude,
                Longitude = newGroup.Longitude,
                Radius = newGroup.Radius,
                IsActive = true
            };

            areaEntity.ApproximateAddress = this._geoCodingManager.GetApproximateAddress(new NewAreaRequest()
            {
                Latitude = newGroup.Latitude,
                Longitude = newGroup.Longitude
            });

            var newAreaId = this._unitOfWork.AreaRepository.InsertArea(areaEntity);

            this._unitOfWork.AreaRepository.Save();

            // 2. Add new group
            var newGroupEntity = this._mapper.Map<GroupAsNewAreaRequestDto, Group>(newGroup);
            this.AssignBaseGroupInfo(newGroup, newGroupEntity);

            var newlyCreatedArea = this._unitOfWork.AreaRepository.GetAreaById(newAreaId);

            newGroupEntity.Areas.Add(newlyCreatedArea);
            
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

        public IEnumerable<GroupMetadataResponseDto> AllGroups()
        {
            var result = this._unitOfWork.GroupRepository.GetGroups();

            var groupsAsDtos = this._mapper.Map<IEnumerable<Group>, IEnumerable<GroupMetadataResponseDto>>(result);

            return groupsAsDtos;
        }

        public IEnumerable<Guid> GetUserSubscriptions(Guid uid, double lat, double lon)
        {
            var groupsAroundCoordinates = this.GetGroups(lat, lon).Select(gr => gr.Id);

            var user = this._unitOfWork.UserRepository.GetUserById(uid);

            if(user == null)
                throw new ArgumentException($"User with id {uid} does not exist in the DB.");

            if (user.Groups != null && user.Groups.Any())
            {
                var allGroupsForUser = user.Groups.Select(gr => gr.Id);

                return allGroupsForUser.Where(gr => groupsAroundCoordinates.Contains(gr));
            }

            return new List<Guid>() { };
        }

        public PreGroupCreationResponseDto GetGroupCreationType(double lat, double lon, Guid userId)
        {
            var currentLocation = new GeoCoordinate(lat, lon);

            var areaEntities = this._unitOfWork.AreaRepository.GetAreas()
                .Where(x => /*x.IsActive &&*/ currentLocation.GetDistanceTo(new GeoCoordinate(x.Latitude, x.Longitude)) <= (int)x.Radius);

            var areas = areaEntities as Area[] ?? areaEntities.ToArray();

            if (!areas.Any())
                return new PreGroupCreationResponseDto()
                {
                    CreationType = CreateGroupActionTypeEnum.AutomaticallyGroupAsNewArea,
                    PrevousGroupsList = null
                };

            var inactiveGroupsInTheArea =
                areas.SelectMany(a => a.Groups).Distinct().Where(gr => gr.IsActive == false).ToList();

            if(!inactiveGroupsInTheArea.Any() && areas.Any(a => a.IsActive))
                return new PreGroupCreationResponseDto()
                {
                    CreationType = CreateGroupActionTypeEnum.ChoiceScreen,
                    PrevousGroupsList = null
                };

            return this.GetListOfPrevouslyCreatedOrSubscribedGroups(inactiveGroupsInTheArea, areas, userId);
        }

        public void ActivateGroup(Guid groupId, Guid userId)
        {
            var affectedGroup = this._unitOfWork.GroupRepository.GetGroupById(groupId);

            var inactiveAreasThisGroupBelongsTo = affectedGroup.Areas.Where(a => a.IsActive == false);

            foreach (var inactiveArea in inactiveAreasThisGroupBelongsTo)
            {
                inactiveArea.IsActive = true;
            }

            affectedGroup.IsActive = true;

            var user = this._unitOfWork.UserRepository.GetUserById(userId);

            user.Groups.Add(affectedGroup);

            this._unitOfWork.Save();
        }

        #region Private area
        private PreGroupCreationResponseDto GetListOfPrevouslyCreatedOrSubscribedGroups(
            IEnumerable<Group> inactiveGroupsInTheArea,
            IEnumerable<Area> areas,
            Guid userId)
        {
            var groupsUserPreviouslyInteractedWith = this._unitOfWork
                .UserRepository.GetUserById(userId)
                .HistoryGroups.Select(hgr => hgr.GroupId).ToList();

            var groupsToReturn = inactiveGroupsInTheArea
                .Where(gr => groupsUserPreviouslyInteractedWith.Contains(gr.Id))
                .Select(g => new RecreateGroupRecord()
                {
                    GroupName = g.Name,
                    ReCreateGroupUrl = string.Format(GroupRecreateUrlTemplate, g.Id)
                }).ToList();

            if (!groupsToReturn.Any())
            {
                return new PreGroupCreationResponseDto()
                {
                    CreationType = areas.Any(a => a.IsActive) ? CreateGroupActionTypeEnum.ChoiceScreen : CreateGroupActionTypeEnum.AutomaticallyGroupAsNewArea,
                    PrevousGroupsList = null
                };
            }

            return new PreGroupCreationResponseDto()
            {
                CreationType = CreateGroupActionTypeEnum.ListOfPreviouslyCreatedOrSubscribedGroups,
                PrevousGroupsList = groupsToReturn
            };
        }

        private void AssignBaseGroupInfo(BaseNewGroupRequest newGroup, Group newGroupEntity)
        {
            newGroupEntity.Id = Guid.NewGuid();

            newGroupEntity.Areas = new List<Area>();
            newGroupEntity.Tags = new List<Tag>();
            newGroupEntity.Users = new List<User>();

            if (newGroup.UserId != Guid.Empty)
            {
                var user = this._unitOfWork.UserRepository.GetUserById(newGroup.UserId);

                if (user != null)
                {
                    newGroupEntity.Users.Add(user);

                    if (user.HistoryGroups == null)
                        user.HistoryGroups = new List<HistoryGroup>();

                    user.HistoryGroups.Add(new HistoryGroup()
                    {
                        DateTimeGroupWasSubscribed = DateTime.UtcNow,
                        GroupId = newGroupEntity.Id,
                        GroupThatWasPreviouslySubscribed = newGroupEntity,
                        Id = Guid.NewGuid(),
                        UserId = newGroup.UserId,
                        UserWhoSubscribedGroup = user
                    });
                }
            }

            newGroupEntity.IsActive = true;

            if (newGroup.TagIds != null && newGroup.TagIds.Any())
            {
                var tagsForGroup = this._unitOfWork.TagRepository.GetAllTags().Where(t => newGroup.TagIds.Contains(t.Id));
                foreach (var tag in tagsForGroup)
                {
                    newGroupEntity.Tags.Add(tag);
                }
            }
        }
        #endregion
    }
}
