using System.Web.Http;
using Likkle.BusinessServices;

namespace Likkle.Api.Controllers
{
    public class ValuesController : ApiController
    {
        private DataService _likkleDataService;

        public ValuesController()
        {
            this._likkleDataService = new DataService();
        }

        // GET api/values
        [HttpGet]
        public IHttpActionResult Get()
        {
            var result = this._likkleDataService.GetAllAreas();

            return Ok(result);
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
