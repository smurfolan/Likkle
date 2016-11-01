using System.Collections.Generic;

namespace Likkle.DataModel.Repositories
{
    public interface ITagRepository
    {
        IEnumerable<Tag> GetAllTags();
    }
}
