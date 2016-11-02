using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Requests;
using Likkle.DataModel;
using Likkle.DataModel.UnitOfWork;

namespace Likkle.BusinessServices
{
    public class DataService : IDataService
    {
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

        public IEnumerable<AreaDto> GetAreas(double latitude, double longitude)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserDto> GetUsersFromArea(Guid areaId)
        {
            var users = this._unitOfWork.AreaRepository.GetAreas()
                .Where(a => a.Id == areaId)
                .SelectMany(ar => ar.Groups.SelectMany(gr => gr.Users));

            var usersAsDtos = this._mapper.Map<IEnumerable<User>, IEnumerable<UserDto>>(users);

            return usersAsDtos;
        }

        public Guid InsertNewArea(NewAreaRequest newArea)
        {
            var areaEntity = this._mapper.Map<NewAreaRequest, Area>(newArea);
            
            return this._unitOfWork.AreaRepository.Insert(areaEntity);
        }

        #endregion

        #region Group specific

        public IEnumerable<GroupDto> GetGroups(double latitude, double longitude)
        {
            var result = this._unitOfWork.AreaRepository.GetAreas(latitude, longitude);

            var groupsInsideAreas = result.SelectMany(ar => ar.Groups);

            var groupsAsDtos = this._mapper.Map<IEnumerable<Group>, IEnumerable<GroupDto>>(groupsInsideAreas);

            return groupsAsDtos;
        }

        // TODO: Extract common parts from InsertNewGroup and InserGroupAsNewArea
        public Guid InsertNewGroup(StandaloneGroupRequestDto newGroup)
        {
            var newGroupEntity = this._mapper.Map<StandaloneGroupRequestDto, Group>(newGroup);
            newGroupEntity.Areas = new List<Area>();
            newGroupEntity.Tags = new List<Tag>();

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

    }
}
