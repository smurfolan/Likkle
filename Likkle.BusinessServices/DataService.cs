using System;
using System.Collections.Generic;
using System.Linq;
using Likkle.BusinessEntities;
using Likkle.DataModel;
using Likkle.DataModel.UnitOfWork;

namespace Likkle.BusinessServices
{
    public class DataService : IDataService
    {
        private readonly LikkleUoW _unitOfWork;

        public DataService()
        {
            _unitOfWork = new LikkleUoW();
        }

        public IEnumerable<AreaDto> GetAllAreas()
        {
            return this._unitOfWork.AreaRepository.GetAreas().Select(a => new AreaDto()
            {
                Id = a.Id,
                Latitude = a.Latitude,
                Longitude = a.Longitude,
                Groups = a.Groups.Select(g => new GroupDto() {Id = g.Id, Name = g.Name})
            }).ToList();
        }

        public Guid InsertNewArea(AreaDto newArea)
        {
            var areaEntity = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = newArea.Latitude,
                Longitude = newArea.Longitude
            };
            return this._unitOfWork.AreaRepository.Insert(areaEntity);
        }
    }
}
