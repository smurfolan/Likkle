using System;
using System.Collections.Generic;
using Likkle.BusinessEntities;

namespace Likkle.BusinessServices
{
    public interface IDataService
    {
        IEnumerable<AreaDto> GetAllAreas();
        Guid InsertNewArea(AreaDto newArea);
    }
}
