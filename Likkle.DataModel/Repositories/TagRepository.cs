using System.Collections.Generic;

namespace Likkle.DataModel.Repositories
{
    public class TagRepository : ITagRepository
    {
        private LikkleDbContext _dbContext;

        private bool _disposed = false;

        public TagRepository(LikkleDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public IEnumerable<Tag> GetAllTags()
        {
            return this._dbContext.Tags;
        }

        public void Save()
        {
            this._dbContext.SaveChanges();
        }      
    }
}
