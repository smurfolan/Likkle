using System;
using System.Collections.Generic;
using Likkle.BusinessEntities.SignalrDtos;
using Likkle.BusinessServices.Hubs;
using Microsoft.AspNet.SignalR;
using Likkle.BusinessServices.Utils;

namespace Likkle.BusinessServices
{
    public class SignalrService : ISignalrService
    {
        private readonly IHubContext _groupsActivityHub;
        private readonly ILikkleApiLogger _apiLogger;

        public SignalrService(ILikkleApiLogger apiLogger)
        {
            _apiLogger = apiLogger;
            this._groupsActivityHub = GlobalHost.ConnectionManager.GetHubContext<BoongalooGroupsActivityHub>();
        }

        public void GroupAroundMeWasRecreated(
            string groupId, 
            IEnumerable<SRAreaDto> areaDtos, 
            SRGroupDto groupDto, 
            bool isSubscribedByMe)
        {
            this._groupsActivityHub.Clients
                    .Group(groupId)
                    .groupAroundMeWasRecreated(areaDtos, groupDto, isSubscribedByMe);
        }

        public void GroupAsNewAreaWasCreatedAroundMe(
            string groupId, 
            SRAreaDto areaDto, 
            SRGroupDto groupDto, 
            bool isSubscribedByMe)
        {
            this._groupsActivityHub.Clients
                    .Group(groupId)
                    .groupAsNewAreaWasCreatedAroundMe(areaDto, groupDto, isSubscribedByMe);

            // TEST
            _apiLogger.LogInfo($"User with id {groupId} was pinged by SignalR with event name: GroupAsNewAreaWasCreatedAroundMe");
            // TEST
        }

        public void GroupAttachedToExistingAreasWasCreatedAroundMe(
            string groupId, 
            IEnumerable<Guid> areaIds, 
            SRGroupDto groupDto,
            bool isSubscribedByMe)
        {
            this._groupsActivityHub.Clients
                    .Group(groupId)
                    .groupAttachedToExistingAreasWasCreatedAroundMe(areaIds, groupDto, isSubscribedByMe);

            // TEST
            _apiLogger.LogInfo($"User with id {groupId} was pinged by SignalR with event name: GroupAttachedToExistingAreasWasCreatedAroundMe");
            // TEST
        }

        public void GroupWasJoinedByUser(Guid groupId, List<string> usersToBeNotified)
        {
            this._groupsActivityHub.Clients
                .Groups(usersToBeNotified)
                .groupWasJoinedByUser(groupId);

            // TEST
            _apiLogger.LogInfo($"User with id {groupId} was pinged by SignalR with event name: GroupWasJoinedByUser");
            // TEST
        }

        public void GroupWasLeftByUser(
            Guid groupId,
            List<string> usersToBeNotified)
        {
            this._groupsActivityHub.Clients
                .Groups(usersToBeNotified)
                .groupWasLeftByUser(groupId);

            // TEST
            _apiLogger.LogInfo($"User with id {groupId} was pinged by SignalR with event name: GroupWasLeftByUser");
            // TEST
        }
    }
}
