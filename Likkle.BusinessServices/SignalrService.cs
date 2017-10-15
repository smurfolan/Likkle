using System;
using System.Collections.Generic;
using Likkle.BusinessEntities.SignalrDtos;
using Likkle.BusinessServices.Hubs;
using Microsoft.AspNet.SignalR;

namespace Likkle.BusinessServices
{
    public class SignalrService : ISignalrService
    {
        private readonly IHubContext _groupsActivityHub;

        public SignalrService()
        {
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
        }

        public void GroupWasJoinedByUser(
            Guid groupId, 
            List<string> usersToBeNotified)
        {
            this._groupsActivityHub.Clients
                .Groups(usersToBeNotified)
                .groupWasJoinedByUser(groupId);
        }

        public void GroupWasLeftByUser(
            Guid groupId,
            List<string> usersToBeNotified)
        {
            this._groupsActivityHub.Clients
                .Groups(usersToBeNotified)
                .groupWasLeftByUser(groupId);
        }
    }
}
