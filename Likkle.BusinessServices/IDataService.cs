using System;
using System.Collections.Generic;
using Likkle.BusinessEntities;

namespace Likkle.BusinessServices
{
    public interface IDataService
    {
        #region Area specific
        IEnumerable<AreaDto> GetAllAreas();
        AreaDto GetAreaById(Guid areaId);
        IEnumerable<AreaDto> GetAreasForGroupId(Guid groupId);
        IEnumerable<AreaDto> GetAreas(double latitude, double longitude);

        Guid InsertNewArea(AreaDto newArea);      
        #endregion
    }
}
