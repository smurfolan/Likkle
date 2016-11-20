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
        private readonly IDataService _likkleDataService;
        private readonly ILikkleApiLogger _apiLogger;

        public UserController(
            IDataService dataService, 
            ILikkleApiLogger logger)
        {
            this._likkleDataService = dataService;
            this._apiLogger = logger;
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
                _apiLogger.LogError("Error while getting user by id.", ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Example: GET api/v1/users/bystsid/{stsId:string}
        /// </summary>
        /// <param name="stsId">Base64 encoded STS id.</param>
        /// <returns>If there's such user in the system - UserDto is returned, otherwise null.</returns>
        [HttpGet]
        [Route("bystsid/{stsId}")]
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
                _apiLogger.LogError("Error while getting user by STS id.", ex);
                return InternalServerError();
            }

        }

        /// <summary>
        /// Example: POST api/v1/users/ChangeGroupsSubscribtion
        /// </summary>
        /// <param name="userToGroupsModel">Body sample: {'userId':'9fa631dd-7d0d-4235-b330-baf23862d90b', 'groupsUserSubscribes':['7dbb6004-9302-44ca-9a3c-7175c91b0094', '72f3a2cf-a9ab-4f93-a581-7ae07e812ef4']}</param>
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
                _apiLogger.LogError("Error while subscribing user to groups.", ex);
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

                return Created("api/v1/users/" + newlyCreatedUserId, "Success");
            }
            catch (Exception ex)
            {
                _apiLogger.LogError("Error while inserting a new user.", ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Example: PUT api/v1/users/{id:Guid}
        /// </summary>
        /// <param name="id">Unique identifier of the user that will be updated</param>
        /// <param name="updateUserData">Updated user data. Body sample: {'firstName': 'Stefcho', 'lastName': 'Stefchev', 'email': 'used@to.know', 'about': 'Straightforward', 'gender': '0', 'birthDate': '0001-01-01T00:00:00', 'phoneNumber': '+395887647288', 'languageIds' : ['72f3a2cf-a9ab-4f93-a581-7ae07e812ef4','72f3a2cf-a9ab-4f93-a581-7ae07e812ef1']</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult Put(Guid id, [FromBody]UpdateUserInfoRequestDto updateUserData)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                this._likkleDataService.UpdateUserInfo(id, updateUserData);

                return Ok();
            }
            catch (Exception ex)
            {
                _apiLogger.LogError("Error while updating user.", ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Example: GET api/v1/users/{id:Guid}/subscriptions
        /// </summary>
        /// <param name="id">Id of the user we want to get the subscribtions</param>
        /// <returns>List of Guids which indicate the group ids to which the user is subscribed</returns>
        [HttpGet]
        [Route("{id}/subscriptions")]
        public IHttpActionResult GetSubscriptions(Guid id)
        {
            try
            {
                var result = this._likkleDataService.GetUserSubscriptions(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _apiLogger.LogError("Error getting user subscribtions.", ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Example: PUT api/v1/users/{id:Guid}/UpdateNotifications
        /// </summary>
        /// <param name="id">Unique identifier of the user whose notification settings we are updating</param>
        /// <param name="notifications">Body sample: {"automaticallySubscribeToAllGroups" : true, "automaticallySubscribeToAllGroupsWithTag" :  false, "subscribedTagIds" : ['72f3a2cf-a9ab-4f93-a581-7ae07e812ef4', '72f3a2cf-a9ab-4f93-a581-7ae07e812234']}</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id}/UpdateNotifications")]
        public IHttpActionResult Put(Guid id, [FromBody] EditUserNotificationsRequestDto notifications)
        {
            // TODO: Add validation which asserts that AutomaticallySubscribeToAllGroups != AutomaticallySubscribeToAllGroupsWithTag
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                this._likkleDataService.UpdateUserNotificationSettings(id, notifications);

                return Ok();
            }
            catch (Exception ex)
            {
                _apiLogger.LogError("Error updating user notification settings.", ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Example: api/v1/users/{id:Guid}/NotificationSettings
        /// </summary>
        /// <param name="id">Unique identifier for user. NOT Identity Server Id.</param>
        /// <returns>Latest notification settings set up by the user.</returns>
        [HttpGet]
        [Route("{id}/NotificationSettings")]
        public IHttpActionResult GetNotificationSettings(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                var result = this._likkleDataService.GetNotificationSettingsForUserWithId(id);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _apiLogger.LogError("Error getting user notification settings.", ex);
                return InternalServerError();
            }
        }
    }
}
