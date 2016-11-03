using System.Collections.Generic;

namespace Likkle.DataModel.Repositories
{
    public interface ILanguageRepository
    {
        IEnumerable<Language> GetAlLanguages();
    }
}
