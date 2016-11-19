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

        public AreaController(IDataService dataService)
        {
            this._likkleDataService = dataService;
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

                return Ok(result);
            }
            catch (Exception ex)
            {
                LikkleApiLogger.LogError("Error while getting are by its id.", ex);
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
        [Route("{lat:double}/{lon:double}")]
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
                LikkleApiLogger.LogError("Error while getting groups around coordinates.", ex);
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
                LikkleApiLogger.LogError("Error while getting users for area.", ex);
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
                LikkleApiLogger.LogError("Error while creating new area.", ex);
                return InternalServerError();
            }
        }

    }
}
