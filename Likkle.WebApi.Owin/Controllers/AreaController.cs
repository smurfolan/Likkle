using System;
using System.Web.Http;
using Likkle.BusinessServices;

namespace Likkle.WebApi.Owin.Controllers
{
    [Authorize]
    [RoutePrefix("api/v1/areas")]
    public class AreaController : ApiController
    {
        private readonly DataService _likkleDataService;

        public AreaController()
        {
            this._likkleDataService = new DataService();
        }

        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult Get(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = this._likkleDataService.GetAreaById(id);

            return Ok(result);
        }
    }
}
