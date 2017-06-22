using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using AutoMapper;
using Likkle.DataModel;
using Likkle.DataModel.UnitOfWork;

namespace Likkle.BusinessServices
{
    public class SubscriptionSettingsService : ISubscriptionSettingsService
    {
        private readonly ILikkleUoW _unitOfWork;
        private readonly IMapper _mapper;

        public SubscriptionSettingsService(ILikkleUoW unitOfWork, IConfigurationProvider configurationProvider)
        {
            _unitOfWork = unitOfWork;
            _mapper = configurationProvider.CreateMapper();
        }

        public IEnumerable<Guid> GroupsForUserAroundCoordinatesBasedOnUserSettings(
            Guid userId, 
            double latitude, 
            double longitude)
        {
            var user = this._unitOfWork.UserRepository.GetUserById(userId);

            if(user == null)
                throw new ArgumentException("User with such id does not exist in database.");

            var userSettings = user.NotificationSettings;

            if(userSettings == null)
                throw new ArgumentException($"There's no user settings related to user with ID:{userId}");

            if (userSettings.AutomaticallySubscribeToAllGroups)
                return GetAllActivGroupsAroundCoordinates(latitude, longitude);

            if (userSettings.AutomaticallySubscribeToAllGroupsWithTag)
                return GetAllActiveGroupsWithTags(latitude, longitude, userSettings.Tags);

            return new List<Guid>();
        }

        private IEnumerable<Guid> GetAllActiveGroupsWithTags(
            double latitude, 
            double longitude, 
            ICollection<Tag> userSettingsTags)
        {
            var currentLocation = new GeoCoordinate(latitude, longitude);

            var areaEntities = this._unitOfWork.AreaRepository.GetAreas()
                .Where(x => x.IsActive && currentLocation.GetDistanceTo(new GeoCoordinate(x.Latitude, x.Longitude)) <= (int)x.Radius);

            return areaEntities.SelectMany(a => a.Groups)
                .Where(
                        gr => gr.IsActive 
                        && gr.Tags.Select(t => t.Id).Intersect(userSettingsTags.Select(ta => ta.Id)).Any())  
                .ToList().Select(gr => gr.Id).ToList();
        }

        private IEnumerable<Guid> GetAllActivGroupsAroundCoordinates(
            double latitude, 
            double longitude)
        {
            var currentLocation = new GeoCoordinate(latitude, longitude);

            var areaEntities = this._unitOfWork.AreaRepository.GetAreas()
                .Where(x => x.IsActive && currentLocation.GetDistanceTo(new GeoCoordinate(x.Latitude, x.Longitude)) <= (int)x.Radius);

            return areaEntities
                .SelectMany(a => a.Groups)
                .Where(gr => gr.IsActive)
                .Select(gr => gr.Id).ToList();
        }
    }
}
