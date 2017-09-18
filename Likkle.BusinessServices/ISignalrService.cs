using System;
using System.Collections.Generic;
using Likkle.BusinessEntities.SignalrDtos;

namespace Likkle.BusinessServices
{
    public interface ISignalrService
    {
        void GroupAroundMeWasRecreated(
            string groupId, 
            IEnumerable<SRAreaDto> areaDtos, 
            SRGroupDto groupDto,
            bool isSubscribedByMe);

        void GroupAsNewAreaWasCreatedAroundMe(
            string groupId, 
            SRAreaDto areaDto, 
            SRGroupDto groupDto,
            bool isSubscribedByMe);

        void GroupAttachedToExistingAreasWasCreatedAroundMe(
            string groupId, 
            IEnumerable<Guid> areaIds,
            SRGroupDto groupDto, 
            bool isSubscribedByMe);
    }
}
