using System;
using System.Collections.Generic;

namespace Likkle.DataModel.Repositories
{
    public interface IGroupRepository
    {
        IEnumerable<Group> GetGroups();
        Group GetGroupById(Guid groupId);
        IEnumerable<Group> GetGroupsForUserId(Guid userId);
        IEnumerable<Group> GetGroupsForAreaId(Guid areaId);

        Guid InsertGroup(Group groupToInsert);
        void DeleteGroup(Guid groupId);
        void UpdateGroup(Group updatedGroup);
    }
}
