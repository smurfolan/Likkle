using System;
using System.Linq;
using System.Web.Http;
using Likkle.BusinessServices;
using Likkle.Helpers;

namespace Likkle.Controllers
{
    public class AreaController : ApiController
    {
        private DataService likkleDataService;

        public AreaController()
        {
            this.likkleDataService = new DataService();
        }

        [HttpGet]
        public IHttpActionResult Get()
        {
            try
            {
                LikkleLogger.LogInfo("Inside try/catch block");

                var result = this.likkleDataService.GetAllAreas();

                LikkleLogger.LogInfo("Resulte was returned by the service. First record id:" + result.FirstOrDefault().Id);

                return Ok(result);
            }
            catch (Exception ex)
            {
                LikkleLogger.LogInfo("Inside catch block");
                LikkleLogger.LogError("Inside catch block", ex);

                return BadRequest();
            }          
        }
    }
}
