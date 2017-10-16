using System;
using System.Collections.Generic;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessEntities.Responses;

namespace Likkle.BusinessServices
{
    public interface IAreaService
    {
        IEnumerable<AreaDto> GetAllAreas();
        AreaDto GetAreaById(Guid areaId);
        IEnumerable<AreaForLocationResponseDto> GetAreas(double latitude, double longitude);
        IEnumerable<UserDto> GetUsersFromArea(Guid areaId);
        AreaMetadataResponseDto GetMetadataForArea(double latitude, double longitude, Guid areaId);
        IEnumerable<AreaMetadataResponseDto> GetMultipleAreasMetadata(MultipleAreasMetadataRequestDto areas);
        IEnumerable<AreaForLocationResponseDto> GetAreas(double lat, double lon, int rad);
        Guid InsertNewArea(NewAreaRequest newArea);
        IEnumerable<Guid> GetUsersFallingUnderSpecificAreas(IEnumerable<Guid> areaIds);
    }
}
