using System;
using System.Collections.Generic;

namespace Likkle.DataModel.Repositories
{
    public interface IAreaRepository
    {
        IEnumerable<Area> GetAreas();
        Area GetAreaById(Guid areaId);
        IEnumerable<Area> GetAreasForGroupId(Guid groupId);

        Guid InsertArea(Area area);
        void DeleteArea(Guid areaId);
        void UpdateArea(Area area);

        IEnumerable<Area> GetAreas(double latitude, double longitude);

        void Save();
    }
}
