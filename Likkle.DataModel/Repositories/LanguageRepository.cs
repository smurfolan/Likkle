using System.Collections.Generic;

namespace Likkle.DataModel.Repositories
{
    public class LanguageRepository : ILanguageRepository
    {
        private ILikkleDbContext _dbContext;

        private bool _disposed = false;

        public LanguageRepository(ILikkleDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public IEnumerable<Language> GetAlLanguages()
        {
            return this._dbContext.Languages;
        }

        public void Save()
        {
            this._dbContext.SaveChanges();
        }    
    }
}
