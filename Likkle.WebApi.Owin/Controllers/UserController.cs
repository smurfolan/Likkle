using System;
using System.Text;
using System.Web.Http;
using Likkle.BusinessEntities;
using Likkle.BusinessServices;
using Likkle.WebApi.Owin.Helpers;

namespace Likkle.WebApi.Owin.Controllers
{
    //[Authorize]
    [RoutePrefix("api/v1/users")]
    public class UserController : ApiController
    {
        private readonly DataService _likkleDataService;

        public UserController()
        {
            this._likkleDataService = new DataService();
        }

        /// <summary>
        /// Example: GET /api/v1/users/{id:Guid}
        /// </summary>
        /// <param name="id">Unique identifier of a user</param>
        /// <returns>Specific user by its id.</returns>
        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult Get(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = this._likkleDataService.GetUserById(id);

                return Ok(result);
            }
            catch (Exception ex)
            {
                LikkleApiLogger.LogError("Error while getting user by id.", ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Example: GET api/v1/users/{stsId:string}
        /// </summary>
        /// <param name="stsId">Base64 encoded STS id.</param>
        /// <returns>If there's such user in the system - UserDto is returned, otherwise null.</returns>
        [HttpGet]
        [Route("{stsId}")]
        public IHttpActionResult GetUserByStsId(string stsId)
        {
            try
            {
                var data = Convert.FromBase64String(stsId);
                var decodedString = Encoding.UTF8.GetString(data);

                var result = this._likkleDataService.GetUserByStsId(decodedString);

                return Ok(result);
            }
            catch (Exception ex)
            {
                LikkleApiLogger.LogError("Error while getting user by STS id.", ex);
                return InternalServerError();
            }

        }

        /// <summary>
        /// Example: POST api/v1/users/ChangeGroupsSubscribtion
        /// </summary>
        /// <param name="userToGroupsModel">Body sample: {'userId':1, 'groupsUserSubscribes':[3, 105]}</param>
        /// <returns>Http.OK if the operation was successful or Http.500 if there was an error.</returns>
        [HttpPost]
        [Route("ChangeGroupsSubscribtion")]
        public IHttpActionResult Post([FromBody]RelateUserToGroupsDto userToGroupsModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                this._likkleDataService.RelateUserToGroups(userToGroupsModel);
                return Ok();
            }
            catch (Exception ex)
            {
                LikkleApiLogger.LogError("Error while subscribing user to groups.", ex);
                return InternalServerError();
            }
        }
    }
}
