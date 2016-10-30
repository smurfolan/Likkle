using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace Likkle.DataModel.Repositories
{
    public class AreaRepository
    {
        private LikkleDbContext _dbContext;

        private bool _disposed = false;

        public AreaRepository(LikkleDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public IEnumerable<Area> GetAreas()
        {
            return this._dbContext.Areas.Include(g => g.Groups);
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public Guid Insert(Area newArea)
        {
            this._dbContext.Areas.Add(newArea);

            this._dbContext.SaveChanges();

            return newArea.Id;
        }
    }
}
