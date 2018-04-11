using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using AutoMapper;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessEntities.Responses;
using Likkle.BusinessServices.Utils;
using Likkle.DataModel;
using Likkle.DataModel.UnitOfWork;

namespace Likkle.BusinessServices
{
    public class AreaService : IAreaService
    {
        private readonly ILikkleUoW _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfigurationWrapper _configuration;
        private readonly IGeoCodingManager _geoCodingManager;

        public AreaService(
            ILikkleUoW uow,
            IConfigurationProvider configurationProvider,
            IConfigurationWrapper config, 
            IGeoCodingManager geoCodingManager)
        {
            this._unitOfWork = uow;
            _mapper = configurationProvider.CreateMapper();
            this._configuration = config;
            _geoCodingManager = geoCodingManager;
        }

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

        public IEnumerable<AreaForLocationResponseDto> GetAreas(double latitude, double longitude)
        {
            var currentLocation = new GeoCoordinate(latitude, longitude);

            var areaEntities = this._unitOfWork.AreaRepository.GetAreas()
                .Where(x => x.IsActive && currentLocation.GetDistanceTo(new GeoCoordinate(x.Latitude, x.Longitude)) <= (int)x.Radius);

            var areasAsDtos = this._mapper.Map<IEnumerable<Area>, IEnumerable<AreaForLocationResponseDto>>(areaEntities);

            return areasAsDtos;
        }

        public IEnumerable<AreaForLocationResponseDto> GetAreas(double lat, double lon, int rad)
        {
            var currentLocation = new GeoCoordinate(lat, lon);

            var areaEntities = this._unitOfWork.AreaRepository.GetAreas()
                .Where(a => a.IsActive && currentLocation.GetDistanceTo(new GeoCoordinate(a.Latitude, a.Longitude)) <= rad);

            var areasAsDtos = this._mapper.Map<IEnumerable<Area>, IEnumerable<AreaForLocationResponseDto>>(areaEntities);

            return areasAsDtos;
        }

        public IEnumerable<UserDto> GetUsersFromArea(Guid areaId)
        {
            var users = this._unitOfWork.AreaRepository.GetAreas()
                .Where(a => a.Id == areaId)
                .SelectMany(ar => ar.Groups.SelectMany(gr => gr.Users))
                .Distinct();

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

            return
                areaDtos.Select(
                    a =>
                        GetAreaMetadata(new GeoCoordinate(areas.Latitude, areas.Longitude),
                            new GeoCoordinate(a.Latitude, a.Longitude), a));
        }

        public Guid InsertNewArea(NewAreaRequest newArea)
        {
            var areaEntity = this._mapper.Map<NewAreaRequest, Area>(newArea);
            areaEntity.Id = Guid.NewGuid();

            areaEntity.ApproximateAddress = this._geoCodingManager.GetApproximateAddress(newArea);

            areaEntity.IsActive = true;

            return this._unitOfWork.AreaRepository.Insert(areaEntity);
        }

        public IEnumerable<Guid> GetUsersFallingUnderSpecificAreas(IEnumerable<Guid> areaIds)
        {
            var observedAreas = this._unitOfWork
                .AreaRepository
                .GetAreas()
                .Where(a => areaIds.Contains(a.Id));

            var allUsers = new List<User>();

            foreach (var area in observedAreas)
            {
                var areaCenter = new GeoCoordinate(area.Latitude, area.Longitude);
                var usersToBeAdded = this._unitOfWork.UserRepository
                    .GetAllUsers()
                    .Where(u => areaCenter.GetDistanceTo(new GeoCoordinate(u.Latitude, u.Longitude)) <= (int)area.Radius);

                allUsers.AddRange(usersToBeAdded);
            }

            return allUsers.Select(u => u.Id);
        }

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
            var totalNumberOfParticipants = clickedArea.Groups
                .SelectMany(gr => gr.Users)
                .Distinct()
                .Count();

            // 3. Gather all the uniqe group tags from all the groups
            var allTags = clickedArea.Groups.SelectMany(gr => gr.Tags).Select(t => t.Id).Distinct();

            return new AreaMetadataResponseDto()
            {
                DistanceTo = distance,
                NumberOfParticipants = totalNumberOfParticipants,
                TagIds = allTags,
                ApproximateAddress = clickedArea.ApproximateAddress

            };
        }
        #endregion
    }
}
