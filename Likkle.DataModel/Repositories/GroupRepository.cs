using System;
using System.Collections.Generic;
using System.Linq;

namespace Likkle.DataModel.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private LikkleDbContext _dbContext;

        private bool _disposed = false;

        public GroupRepository(LikkleDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public IEnumerable<Group> GetGroups()
        {
            return this._dbContext.Groups;
        }

        public Group GetGroupById(Guid groupId)
        {
            return this._dbContext.Groups.FirstOrDefault(g => g.Id == groupId);
        }

        public IEnumerable<Group> GetGroupsForUserId(Guid userId)
        {
            return this._dbContext.Groups.Where(gr => gr.Users.Select(u => u.Id).Contains(userId));
        }

        public IEnumerable<Group> GetGroupsForAreaId(Guid areaId)
        {
            return this._dbContext.Groups.Where(gr => gr.Areas.Select(a => a.Id).Contains(areaId));
        }

        public Guid InsertGroup(Group groupToInsert)
        {
            this._dbContext.Groups.Add(groupToInsert);

            this._dbContext.SaveChanges();

            return groupToInsert.Id;
        }

        public void DeleteGroup(Guid groupId)
        {
            var groupToDelete = this._dbContext.Groups.FirstOrDefault(gr => gr.Id == groupId);

            this._dbContext.Groups.Remove(groupToDelete);

            this._dbContext.SaveChanges();
        }

        public void UpdateGroup(Group updatedGroup)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            this._dbContext.SaveChanges();
        }
    }
}
