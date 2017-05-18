using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessEntities.Responses;
using Likkle.DataModel;
using Likkle.DataModel.UnitOfWork;

namespace Likkle.BusinessServices
{
    public class GroupService : IGroupService
    {
        private readonly ILikkleUoW _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfigurationWrapper _configuration;

        public GroupService(
            ILikkleUoW uow,
            IConfigurationProvider configurationProvider,
            IConfigurationWrapper config)
        {
            this._unitOfWork = uow;
            _mapper = configurationProvider.CreateMapper();
            this._configuration = config;
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

        // TODO: Extract common parts from InsertNewGroup and InserGroupAsNewArea
        public Guid InsertNewGroup(StandaloneGroupRequestDto newGroup)
        {
            var newGroupEntity = this._mapper.Map<StandaloneGroupRequestDto, Group>(newGroup);
            newGroupEntity.Id = Guid.NewGuid();

            newGroupEntity.Areas = new List<Area>();
            newGroupEntity.Tags = new List<Tag>();
            newGroupEntity.Users = new List<User>();

            if (newGroup.UserId != Guid.Empty)
            {
                var user = this._unitOfWork.UserRepository.GetUserById(newGroup.UserId);

                if (user != null)
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

            newGroupEntity.IsActive = true;

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
                Radius = newGroup.Radius,
                IsActive = true
            };

            var newAreaId = this._unitOfWork.AreaRepository.InsertArea(areaEntity);

            this._unitOfWork.AreaRepository.Save();

            // 2. Add new group
            var newGroupEntity = this._mapper.Map<GroupAsNewAreaRequestDto, Group>(newGroup);
            newGroupEntity.Id = Guid.NewGuid();

            newGroupEntity.Tags = new List<Tag>();
            newGroupEntity.Areas = new List<Area>();
            newGroupEntity.Users = new List<User>();

            if (newGroup.UserId != Guid.Empty)
            {
                var user = this._unitOfWork.UserRepository.GetUserById(newGroup.UserId);

                if (user != null)
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

            newGroupEntity.IsActive = true;

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

            var allGroupsForUser = this._unitOfWork.UserRepository.GetUserById(uid).Groups.Select(gr => gr.Id);

            return allGroupsForUser.Where(gr => groupsAroundCoordinates.Contains(gr));
        }
    }
}
