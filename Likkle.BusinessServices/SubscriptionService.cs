using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using AutoMapper;
using Likkle.BusinessEntities;
using Likkle.DataModel;
using Likkle.DataModel.UnitOfWork;

namespace Likkle.BusinessServices
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ILikkleUoW _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfigurationWrapper _configuration;

        public SubscriptionService(
            ILikkleUoW uow,
            IConfigurationProvider configurationProvider,
            IConfigurationWrapper config)
        {
            this._unitOfWork = uow;
            _mapper = configurationProvider.CreateMapper();
            this._configuration = config;
        }

        public void RelateUserToGroups(RelateUserToGroupsDto newRelations)
        {
            var user = this._unitOfWork.UserRepository.GetUserById(newRelations.UserId);

            if (user == null)
                throw new ArgumentException("User with id:" + newRelations.UserId + " doesn't exist.");

            if (user.Groups == null)
                user.Groups = new List<Group>();

            if (user.HistoryGroups == null)
                user.HistoryGroups = new List<HistoryGroup>();

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
                forUserAroundCurrentLocation.Where(gr => !groupSubscriptionsThatCameFromTheRequest.Contains(gr)).ToList();

            // 4. Determine what groups have bee added and are new: Everything that is in (2) and doesn't belong to (1)
            var newlySubscribedGroups = groupSubscriptionsThatCameFromTheRequest.Where(gr => !forUserAroundCurrentLocation.Contains(gr));

            // 5. Each group in (3) has to be removed from current user subscriptions
            this.DisconnectUserFromUnsubscribedGroups(unsubscribedGroups, user);

            // 6. Each group in (4) has to be added to the current user subscriptions
            this.ConnectUserToNewlySubscribedGroups(newlySubscribedGroups, user);

            this._unitOfWork.Save();

            if (this._configuration.AutomaticallyCleanupGroupsAndAreas)
            {
                this.DeactivateGroupsWithNoUsersInsideOfThem(unsubscribedGroups, newRelations.UserId);
                this._unitOfWork.Save();
            } 
        }

        public void UpdateLatestWellKnownUserLocation(decimal latitude, decimal longitude)
        {
            throw new NotImplementedException();
        }

        #region Private methods
        private void DeactivateGroupsWithNoUsersInsideOfThem(IEnumerable<Group> unsubscribedGroups, Guid userId)
        {
            var userToBeRemoved = this._unitOfWork.UserRepository.GetUserById(userId);

            foreach (var unsubscribedGroup in unsubscribedGroups)
            {
                unsubscribedGroup.Users.Remove(userToBeRemoved);
                if (!unsubscribedGroup.Users.Any())
                {
                    unsubscribedGroup.IsActive = false;
                }

                this.DeactivateAreasWithNoGroups(unsubscribedGroup.Areas);
            }
        }

        private void DeactivateAreasWithNoGroups(IEnumerable<Area> areas)
        {
            foreach (var area in areas)
            {
                if (area.Groups.All(gr => gr.IsActive == false))
                    area.IsActive = false;
            }
        }

        private void DisconnectUserFromUnsubscribedGroups(List<Group> unsubscribedGroups, User user)
        {
            foreach (var unsubscribedGroup in unsubscribedGroups)
            {
                user.Groups.Remove(unsubscribedGroup);

                //var historyRecordToBeRemoved = user.HistoryGroups.FirstOrDefault(hgr => hgr.GroupId == unsubscribedGroup.Id);

                //if (historyRecordToBeRemoved != null)
                //    this._unitOfWork.HistoryGroupRepository.DeleteHistoryGroup(historyRecordToBeRemoved.Id);
            }
        }

        private void ConnectUserToNewlySubscribedGroups(IEnumerable<Group> newlySubscribedGroups, User user)
        {
            foreach (var newlySubscribedGroup in newlySubscribedGroups)
            {
                user.Groups.Add(newlySubscribedGroup);
                user.HistoryGroups.Add(new HistoryGroup()
                {
                    DateTimeGroupWasSubscribed = DateTime.UtcNow,
                    GroupId = newlySubscribedGroup.Id,
                    GroupThatWasPreviouslySubscribed = newlySubscribedGroup,
                    UserId = user.Id,
                    UserWhoSubscribedGroup = user
                });
            }
        }

        #endregion
    }
}
