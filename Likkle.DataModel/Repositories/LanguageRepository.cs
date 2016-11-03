using System.Collections.Generic;

namespace Likkle.DataModel.Repositories
{
    public class LanguageRepository : ILanguageRepository
    {
        private LikkleDbContext _dbContext;

        private bool _disposed = false;

        public LanguageRepository(LikkleDbContext dbContext)
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
