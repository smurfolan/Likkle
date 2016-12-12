using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Device.Location;
using System.Linq;

namespace Likkle.DataModel.Repositories
{
    public class AreaRepository : IAreaRepository
    {
        private ILikkleDbContext _dbContext;

        private bool _disposed = false;

        public AreaRepository(ILikkleDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public IEnumerable<Area> GetAreas()
        {
            return this._dbContext.Areas.Include(g => g.Groups);
        }

        public Area GetAreaById(Guid areaId)
        {
            return this._dbContext.Areas.FirstOrDefault(a => a.Id == areaId);
        }

        public IEnumerable<Area> GetAreasForGroupId(Guid groupId)
        {
            return this._dbContext.Areas.Where(a => a.Groups.Any(g => g.Id == groupId));
        }

        public Guid InsertArea(Area area)
        {
            this._dbContext.Areas.Add(area);
            this._dbContext.SaveChanges();

            return area.Id;
        }

        public void DeleteArea(Guid areaId)
        {
            var areaToDelete = this._dbContext.Areas.FirstOrDefault(a => a.Id == areaId);
            this._dbContext.Areas.Remove(areaToDelete);

            this._dbContext.SaveChanges();
        }

        public void UpdateArea(Area area)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Area> GetAreas(double latitude, double longitude)
        {
            var currentUserLocation = new GeoCoordinate(latitude, longitude);

            return this._dbContext.Areas.ToList()
                .Where(x => currentUserLocation.GetDistanceTo(new GeoCoordinate(x.Latitude, x.Longitude)) <= (int)x.Radius);
        }

        public void Save()
        {
            this._dbContext.SaveChanges();
        }

        public Guid Insert(Area newArea)
        {
            this._dbContext.Areas.Add(newArea);

            this._dbContext.SaveChanges();

            return newArea.Id;
        }
    }
}
