using System;
using System.Linq;
using System.Text;
using System.Web.Http;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessServices;
using Likkle.WebApi.Owin.Helpers;

namespace Likkle.WebApi.Owin.Controllers
{
    [Authorize]
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
        /// <param name="userToGroupsModel">Body sample: {'userId':'6dd49a88-525a-4db7-8d89-13922591f328', 'groupsUserSubscribes':['72f3a2cf-a9ab-4f93-a581-7ae07e812ef4']}</param>
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

        /// <summary>
        /// Example: POST api/v1/users
        /// </summary>
        /// <param name="newUser">Body sample: {'idsrvUniqueId' : 'https://boongaloocompanysts/identity78f100e9-9d90-4de8-9d7d', 'firstName': 'Stefcho', 'lastName': 'Stefchev', 'email': 'used@to.know', 'about': 'Straightforward', 'gender': '0', 'birthDate': '0001-01-01T00:00:00', 'phoneNumber': '+395887647288', 'languageIds' : ['72f3a2cf-a9ab-4f93-a581-7ae07e812ef4','72f3a2cf-a9ab-4f93-a581-7ae07e812ef1'], 'groupIds': ['72f3a2cf-a9ab-4f93-a581-7ae07e81wef4']}</param>
        /// <returns>Http status code 201 if user was succesfuly created or 500 if error has occured.</returns>
        [HttpPost]
        [Route("")]
        public IHttpActionResult Post([FromBody]NewUserRequestDto newUser)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                if (this._likkleDataService.GetAllUsers()
                    .Any(x => x.IdsrvUniqueId == newUser.IdsrvUniqueId || x.Email == newUser.Email))
                    return BadRequest();

                var newlyCreatedUserId = this._likkleDataService.InsertNewUser(newUser);

                return Created("Success", "api/v1/users/" + newlyCreatedUserId);
            }
            catch (Exception ex)
            {
                LikkleApiLogger.LogError("Error while inserting a new user.", ex);
                return InternalServerError();
            }
        }
    }
}
