using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Likkle.DataModel.Repositories
{
    public interface IAreaRepository
    {
        IEnumerable<Area> GetAreas();
        void Save();
    }
}
