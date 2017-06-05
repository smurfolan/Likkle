using System;
using System.Collections.Generic;

namespace Likkle.DataModel.Repositories
{
    public interface IHistoryGroupRepository
    {
        void DeleteHistoryGroup(Guid id);
        IEnumerable<HistoryGroup> AllHistoryGroups();
    }
}
