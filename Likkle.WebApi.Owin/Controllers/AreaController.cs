using System;
using System.Web.Http;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessServices;
using Likkle.WebApi.Owin.Helpers;

namespace Likkle.WebApi.Owin.Controllers
{
    [Authorize]
    [RoutePrefix("api/v1/areas")]
    public class AreaController : ApiController
    {
        private readonly IDataService _likkleDataService;
        private readonly ILikkleApiLogger _apiLogger;

        public AreaController(
            IDataService dataService, 
            ILikkleApiLogger logger)
        {
            this._likkleDataService = dataService;
            this._apiLogger = logger;
        }

        /// <summary>
        /// Example: GET /api/v1/areas/{id:Guid}
        /// </summary>
        /// <param name="id">Id of the area.</param>
        /// <returns>Returns area by its id.</returns>
        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult Get(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = this._likkleDataService.GetAreaById(id);

                if(result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _apiLogger.LogError("Error while getting are by its id.", ex);
                return InternalServerError();
            }        
        }

        /// <summary>
        /// Example: GET /api/v1/areas/metadatafor/{lat:double}/{lon:double}/{id:Guid}
        /// </summary>
        /// <param name="lat">User's current location latitude</param>
        /// <param name="lon">User's current location longitude</param>
        /// <param name="areaId">Id of the clicked area.</param>
        /// <returns>Metadata related to the clicked on the map area. The distance is in meters.</returns>
        [HttpGet]
        [Route("metadatafor/{lat}/{lon}/{areaId}")]
        public IHttpActionResult GetAreaMetadata(double lat, double lon, Guid areaId)
        {
            if(Math.Abs(lat) > 90 || Math.Abs(lon) > 90)
                return BadRequest("Latitude and longitude values must be in the [-90, 90] range.");

            try
            {
                var result = this._likkleDataService.GetMetadataForArea(lat, lon, areaId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _apiLogger.LogError("Error while getting metadata for area.", ex);
                return InternalServerError();
            }
        }

        //TODO: Since it is not a good practice to use POST request for getting data, consider optimizing this for GET.
        /// <summary>
        /// EXAMPLE: POST /api/v1/areas/batchmetadata/
        /// </summary>
        /// <param name="areas">Body sample: {'latitude': 11.111, 'longitude': 22.222, areaIds: ['9ee28efe-55f5-45fb-9aa1-600aa8b08114', 'b75ea52f-2b2b-4415-b385-6696cd7b0824']}</param>
        /// <returns>List of metadata for each area that has a contact with the lat/lon point.</returns>
        [HttpPost]
        [Route("batchmetadata")]
        public IHttpActionResult GetMultipleAreasMetadata([FromBody]MultipleAreasMetadataRequestDto areas)
        {
            try
            {
                var result = this._likkleDataService.GetMultipleAreasMetadata(areas);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _apiLogger.LogError("Error while getting metadata for multiple areas.", ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Example: GET /api/v1/areas/{lat:double}/{lon:double}/
        /// </summary>
        /// <param name="lat">Latitude</param>
        /// <param name="lon">Longitude</param>
        /// <returns>All the areas around coordinates.</returns>
        [HttpGet]
        [Route("{lat:double}/{lon:double}/")]
        public IHttpActionResult Get(double lat, double lon)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = this._likkleDataService.GetAreas(lat, lon);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _apiLogger.LogError("Error while getting groups around coordinates.", ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Example: GET api/v1/areas/{id:Guid}/users
        /// </summary>
        /// <param name="id">Id of the area.</param>
        /// <returns>All the users falling into specific area</returns>
        [HttpGet]
        [Route("{id}/users")]
        public IHttpActionResult GetUsers(Guid id)
        {
            try
            {
                var result = this._likkleDataService.GetUsersFromArea(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _apiLogger.LogError("Error while getting users for area.", ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Example: POST api/v1/areas
        /// </summary>
        /// <param name="area">Sample post: {'radius':50, 'latitude': 23.1233123,'longitude': 43.1231232}</param>
        /// <returns>HTTP Status of 201 code if area was successfuly created.</returns>
        [HttpPost]
        [Route("")]
        public IHttpActionResult Post([FromBody]NewAreaRequest area)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var newAreaId = this._likkleDataService.InsertNewArea(area);

                return Created("api/v1/areas/" + newAreaId, "Success");
            }
            catch (Exception ex)
            {
                _apiLogger.LogError("Error while creating new area.", ex);
                return InternalServerError();
            }
        }

    }
}
