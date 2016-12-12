using System.Collections.Generic;

namespace Likkle.DataModel.Repositories
{
    public class TagRepository : ITagRepository
    {
        private ILikkleDbContext _dbContext;

        private bool _disposed = false;

        public TagRepository(ILikkleDbContext dbContext)
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
