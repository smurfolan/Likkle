﻿using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Security.Principal;
using AutoMapper;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Enums;
using Likkle.BusinessEntities.Requests;
using Likkle.DataModel;
using Likkle.DataModel.UnitOfWork;
using Likkle.BusinessEntities.SignalrDtos;
using Likkle.BusinessServices.Utils;

namespace Likkle.BusinessServices
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ILikkleUoW _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfigurationWrapper _configuration;
        private readonly ISignalrService _signalrService;
        private readonly ILikkleApiLogger _apiLogger;

        public SubscriptionService(
            ILikkleUoW uow,
            IConfigurationProvider configurationProvider,
            IConfigurationWrapper config, 
            ISignalrService signalrService, 
            ILikkleApiLogger apiLogger)
        {
            this._unitOfWork = uow;
            _mapper = configurationProvider.CreateMapper();
            this._configuration = config;
            _signalrService = signalrService;
            _apiLogger = apiLogger;
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

        public void UpdateLatestWellKnownUserLocation(double latitude, double longitude, IPrincipal user)
        {
            var issuer = ((System.Security.Claims.ClaimsPrincipal) user).Claims.First(cl => cl.Type == "iss").Value;
            var subject = ((System.Security.Claims.ClaimsPrincipal)user).Claims.First(cl => cl.Type == "sub").Value;

            var stsId = $"{issuer}{subject}";

            var userToBeUpdated = this._unitOfWork.UserRepository.GetUserByStsId(stsId);
            if(userToBeUpdated == null)
                throw new ArgumentException($"User with sts id {stsId} is not available in DB.");

            userToBeUpdated.Latitude = latitude;
            userToBeUpdated.Longitude = longitude;

            this._unitOfWork.Save();
        }

        public void AutoSubscribeUsersFromExistingAreas(
            IEnumerable<Guid> areaIds, 
            StandaloneGroupRequestDto newGroupMetadata, 
            Guid newGroupId,
            Guid invokedByUserId)
        {
            var areas = this._unitOfWork.AreaRepository.GetAreas().Where(a => areaIds.Contains(a.Id)).ToList();
            var users = areas.SelectMany(ar => ar.Groups)
                        .Where(gr => gr.IsActive == true)
                        .SelectMany(gr => gr.Users).Where(u => u.Id != invokedByUserId)
                        .Distinct();

            if (users == null || !users.Any())
                return;

            var groupToSubscribe = this._unitOfWork.GroupRepository.GetGroupById(newGroupId);

            // Subscribe users on the service
            var subscriptionsResult = this.SubscribeUsersNearbyNewGroup(
                users,
                groupToSubscribe,
                newGroupMetadata.TagIds);
            this._unitOfWork.Save();

            // TEST
            _apiLogger.LogInfo($"The number of the subscribed users except for me is: {users.Count()}");
            foreach (var user in users)
            {
                _apiLogger.LogInfo($"User: {user.FirstName}");
            }
            // TEST

            // Use SignalR to notify all the clients that need to receive information about the newly created group.
            var areaDtos = this._mapper.Map<IEnumerable<Area>, IEnumerable<SRAreaDto>>(areas).ToList();
            var groupDto = this._mapper.Map<Group, SRGroupDto>(groupToSubscribe);

            foreach (var subscr in subscriptionsResult)
            {
                this._signalrService.GroupAttachedToExistingAreasWasCreatedAroundMe(
                    subscr.Key.ToString(), 
                    areaDtos.Select(a => a.Id), 
                    groupDto, 
                    subscr.Value);

                // TEST
                _apiLogger.LogInfo($"Message was sent to {subscr.Key}");
                // TEST
            }
        }

        public void AutoSubscribeUsersForGroupAsNewArea(
            Guid areaId, 
            double newAreaLat, 
            double newAreaLon, 
            RadiusRangeEnum newAreaRadius, 
            Guid newGroupId,
            Guid invokedByUserId)
        {
            var newAreaCenter = new GeoCoordinate(newAreaLat, newAreaLon);
            var groupToSubscribe = this._unitOfWork.GroupRepository.GetGroupById(newGroupId);

            var usersFallingUnderTheNewArea = this._unitOfWork.UserRepository.GetAllUsers()
                .Where(u => newAreaCenter.GetDistanceTo(new GeoCoordinate(u.Latitude, u.Longitude)) <= (int)newAreaRadius && (u.Id != invokedByUserId));

            if (usersFallingUnderTheNewArea == null || !usersFallingUnderTheNewArea.Any())
                return;

            // Subscribe users on the service
            var subscriptionsResult = this.SubscribeUsersNearbyNewGroup(
                usersFallingUnderTheNewArea, 
                groupToSubscribe, 
                groupToSubscribe.Tags.Select(gr => gr.Id).ToList());
            this._unitOfWork.Save();

            // TEST
            _apiLogger.LogInfo($"The number of the subscribed users except for me is: {usersFallingUnderTheNewArea.Count()}");
            foreach (var user in usersFallingUnderTheNewArea)
            {
                _apiLogger.LogInfo($"User: {user.FirstName}");
            }
            // TEST

            // Use SignalR to notify all the clients that need to receive information about the newly created group.
            var areaEntity = this._unitOfWork.AreaRepository.GetAreaById(areaId);
            var areaDto = this._mapper.Map<Area, SRAreaDto>(areaEntity);
            var groupDto = this._mapper.Map<Group, SRGroupDto>(groupToSubscribe);

            foreach (var subcr in subscriptionsResult)
            {
                this._signalrService.GroupAsNewAreaWasCreatedAroundMe(
                    subcr.Key.ToString(), 
                    areaDto, 
                    groupDto, 
                    subcr.Value);

                // TEST
                _apiLogger.LogInfo($"Message was sent to {subcr.Key}");
                // TEST
            }
        }

        public void AutoSubscribeUsersForRecreatedGroup(
            IEnumerable<Guid> areaIds, 
            Guid newGroupId,
            Guid invokedByUserId)
        {
            var groupToSubscribe = this._unitOfWork.GroupRepository.GetGroupById(newGroupId);
            var areas = this._unitOfWork.AreaRepository.GetAreas().Where(a => areaIds.Contains(a.Id)).ToList();
            var allUsers = new List<User>();

            foreach (var area in areas)
            {
                var areaCenter = new GeoCoordinate(area.Latitude, area.Longitude);
                var usersToBeAdded = this._unitOfWork.UserRepository
                    .GetAllUsers()
                    .Where(u => areaCenter.GetDistanceTo(new GeoCoordinate(u.Latitude, u.Longitude)) <= (int)area.Radius && (u.Id != invokedByUserId));

                allUsers.AddRange(usersToBeAdded);
            }

            if (allUsers == null || !allUsers.Any())
                return;

            // Subscribe users on the service
            var subscriptionsResult = this.SubscribeUsersNearbyNewGroup(allUsers.Distinct(), groupToSubscribe, groupToSubscribe.Tags.Select(gr => gr.Id));
            this._unitOfWork.Save();

            // Use SignalR to notify all the clients that need to receive information about the recreated group.
            var areaDtos = this._mapper.Map<IEnumerable<Area>, IEnumerable<SRAreaDto>>(areas).ToList();
            var groupDto = this._mapper.Map<Group, SRGroupDto>(groupToSubscribe);

            foreach (var subscr in subscriptionsResult)
            {
                this._signalrService.GroupAroundMeWasRecreated(
                    subscr.Key.ToString(), 
                    areaDtos, 
                    groupDto, 
                    subscr.Value);
            }
        }

        public void AutoIncreaseUsersInGroups(
            IEnumerable<Guid> groupsThatNeedToIncreaseTheNumberOfTheirUsers, 
            Guid invokedByUserId)
        {
            var allAreas = this._unitOfWork.AreaRepository.GetAreas();

            UseSignalrToChangeGroupsParticipantsNumber(
                groupsThatNeedToIncreaseTheNumberOfTheirUsers, 
                invokedByUserId, 
                allAreas,
                true);
        }

        public void AutoDecreaseUsersInGroups(
            IEnumerable<Guid> groupsThatNeedToDecreaseTheNumberOfTheirUsers, 
            Guid invokedByUserId)
        {
            var allAreas = this._unitOfWork.AreaRepository.GetAreas();

            UseSignalrToChangeGroupsParticipantsNumber(
                groupsThatNeedToDecreaseTheNumberOfTheirUsers,
                invokedByUserId,
                allAreas,
                false);
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

            if(unsubscribedGroups.Any())
                this.AutoDecreaseUsersInGroups(unsubscribedGroups.Select(gr => gr.Id).ToList(), user.Id);
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

            if(newlySubscribedGroups.Any())
                this.AutoIncreaseUsersInGroups(newlySubscribedGroups.Select(gr => gr.Id).ToList(), user.Id);
        }

        private Dictionary<Guid, bool> SubscribeUsersNearbyNewGroup(IEnumerable<User> users, Group groupToSubscribe, IEnumerable<Guid> tagIds)
        {
            var result = new Dictionary<Guid, bool>() { };

            foreach (var user in users)
            {
                var autoSubscrSetings = user.AutomaticSubscriptionSettings;
                if (autoSubscrSetings.AutomaticallySubscribeToAllGroups)
                {
                    user.Groups.Add(groupToSubscribe);
                    result.Add(user.Id, true);
                    continue;
                }
                else if (autoSubscrSetings.AutomaticallySubscribeToAllGroupsWithTag)
                {
                    var tagsUserSubscribes = autoSubscrSetings.Tags.Select(t => t.Id);
                    var newGroupTags = tagIds;

                    var thereIsIntersection = tagsUserSubscribes.Intersect(newGroupTags).Any();

                    if (thereIsIntersection)
                    {
                        user.Groups.Add(groupToSubscribe);
                        result.Add(user.Id, true);
                        continue;
                    }
                    else
                    {
                        result.Add(user.Id, false);
                        continue;
                    }
                }
            }

            return result;
        }

        private void UseSignalrToChangeGroupsParticipantsNumber(
            IEnumerable<Guid> groupsThatNeedToIncreaseTheNumberOfTheirUsers,
            Guid invokedByUserId,
            IEnumerable<Area> allAreas,
            bool isIncrementalOperation)
        {
            foreach (var group in groupsThatNeedToIncreaseTheNumberOfTheirUsers)
            {
                var areasIncludingThisGroup = allAreas
                    .Where(a => a.Groups.Select(gr => gr.Id).Contains(group))
                    .ToList();

                var usersToBeNotified = areasIncludingThisGroup
                    .SelectMany(a => a.Groups)
                    .SelectMany(gr => gr.Users)
                    .Where(u => u.Id != invokedByUserId)
                    .Select(u => u.Id.ToString())
                    .Distinct()
                    .ToList();

                if (isIncrementalOperation)
                    this._signalrService.GroupWasJoinedByUser(group, usersToBeNotified);
                else
                    this._signalrService.GroupWasLeftByUser(group, usersToBeNotified);
            }
        }
        #endregion
    }
}
