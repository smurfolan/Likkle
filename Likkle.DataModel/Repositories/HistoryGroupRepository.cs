using System;
using System.Collections.Generic;
using System.Linq;

namespace Likkle.DataModel.Repositories
{
    public class HistoryGroupRepository : IHistoryGroupRepository
    {
        private ILikkleDbContext _dbContext;

        private bool _disposed = false;

        public HistoryGroupRepository(ILikkleDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public void DeleteHistoryGroup(Guid id)
        {
            var historyGroupToRemove = this._dbContext.HistoryGroups.FirstOrDefault(gr => gr.Id == id);
            this._dbContext.HistoryGroups.Remove(historyGroupToRemove);
        }

        public IEnumerable<HistoryGroup> AllHistoryGroups()
        {
            return this._dbContext.HistoryGroups;
        }

        public void Save()
        {
            this._dbContext.SaveChanges();
        }
    }
}
