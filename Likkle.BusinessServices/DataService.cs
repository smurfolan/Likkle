using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Requests;
using Likkle.DataModel;
using Likkle.DataModel.UnitOfWork;

namespace Likkle.BusinessServices
{
    public class DataService : IDataService
    {
        private readonly LikkleUoW _unitOfWork;
        private readonly IMapper _mapper;

        public DataService()
        {
            _unitOfWork = new LikkleUoW();
            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.AddProfile<EntitiesMappingProfile>();
            });
            _mapper = mapperConfiguration.CreateMapper();
        }

        public IEnumerable<AreaDto> GetAllAreas()
        {
            var allAreaEntities = this._unitOfWork.AreaRepository.GetAreas();

            var areasAsDtos = this._mapper.Map<IEnumerable<Area>, IEnumerable<AreaDto>>(allAreaEntities);

            return areasAsDtos;
        }

        public AreaDto GetAreaById(Guid areaId)
        {
            var area = this._unitOfWork.AreaRepository.GetAreaById(areaId);

            var areaAsDto = this._mapper.Map<Area, AreaDto>(area);

            return areaAsDto;
        }

        public IEnumerable<AreaDto> GetAreasForGroupId(Guid groupId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AreaDto> GetAreas(double latitude, double longitude)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserDto> GetUsersFromArea(Guid areaId)
        {
            var users = this._unitOfWork.AreaRepository.GetAreas()
                .Where(a => a.Id == areaId)
                .SelectMany(ar => ar.Groups.SelectMany(gr => gr.Users));

            var usersAsDtos = this._mapper.Map<IEnumerable<User>, IEnumerable<UserDto>>(users);

            return usersAsDtos;
        }

        public Guid InsertNewArea(NewAreaRequest newArea)
        {
            var areaEntity = this._mapper.Map<NewAreaRequest, Area>(newArea);
            
            return this._unitOfWork.AreaRepository.Insert(areaEntity);
        }
    }
}
