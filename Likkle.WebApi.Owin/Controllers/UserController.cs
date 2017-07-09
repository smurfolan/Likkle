using System;
using System.Linq;
using System.Text;
using System.Web.Http;
using FluentValidation;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessEntities.Responses;
using Likkle.BusinessServices;
using Likkle.BusinessServices.Utils;
using Likkle.BusinessServices.Validators;
using Likkle.WebApi.Owin.Helpers;

namespace Likkle.WebApi.Owin.Controllers
{
    [Authorize]
    [RoutePrefix("api/v1/users")]
    public class UserController : ApiController
    {
        private readonly IUserService _userService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IGroupService _groupService;
        private readonly IPhoneValidationManager _phoneValidationManager;

        private readonly ILikkleApiLogger _apiLogger;

        public UserController(
            IUserService userService, 
            ILikkleApiLogger logger, 
            ISubscriptionService subscriptionService, 
            IGroupService groupService,
            IPhoneValidationManager phoneValidationManager)
        {
            this._userService = userService;
            this._apiLogger = logger;
            this._subscriptionService = subscriptionService;
            _groupService = groupService;
            this._phoneValidationManager = phoneValidationManager;
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
                var result = this._userService.GetUserById(id);

                if (result == null)
                    return NotFound();

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

                var result = this._userService.GetUserByStsId(decodedString);

                if (result == null)
                    return NotFound();

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
        /// <param name="userToGroupsModel">Body sample: {'userId':'9fa631dd-7d0d-4235-b330-baf23862d90b', 'latitude':42.657064, 'longitude':23.28539, 'groupsUserSubscribes':['7dbb6004-9302-44ca-9a3c-7175c91b0094', '72f3a2cf-a9ab-4f93-a581-7ae07e812ef4']}</param>
        /// <returns>Http.OK if the operation was successful or Http.500 if there was an error.</returns>
        [HttpPost]
        [Route("ChangeGroupsSubscribtion")]
        public IHttpActionResult Post([FromBody]RelateUserToGroupsDto userToGroupsModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                this._subscriptionService.RelateUserToGroups(userToGroupsModel);
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
        /// <param name="newUser">Body sample: {'idsrvUniqueId' : 'https://boongaloocompanysts/identity78f100e9-9d90-4de8-9d7d', 'firstName': 'Stefcho', 'lastName': 'Stefchev', 'email': 'used@to.know', 'about': 'Straightforward', 'gender': '0', 'birthDate': '0001-01-01T00:00:00', 'phoneNumber': '+395887647288', 'languageIds' : ['e9260fb3-5183-4c3e-9bd2-c606d03b7bcb','05872235-365b-41f8-ab50-3913ffe9c601'], 'groupIds': ['22811e2c-8c9c-432b-aa8e-0a9668d8cb28']}</param>
        /// <returns>Http status code 201 if user was succesfuly created or 500 if error has occured.</returns>
        [HttpPost]
        [Route("")]
        public IHttpActionResult Post([FromBody]NewUserRequestDto newUser)
        {
            var validator = new NewUserRequestDtoValidator(this._phoneValidationManager, this._userService);
            var results = validator.Validate(newUser);

            var detailedError = new StringBuilder();
            foreach (var error in results.Errors.Select(e => e.ErrorMessage))
            {
                detailedError.Append(error + "; ");
            }

            if (!results.IsValid)
                return BadRequest(detailedError.ToString()); // TODO: Think of returning the errors in a better way

            try
            {
                var newlyCreatedUserId = this._userService.InsertNewUser(newUser);

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
        /// <param name="updateUserData">Updated user data. Body sample: {'firstName': 'Stefcho', 'lastName': 'Stefchev', 'email': 'used@to.know', 'about': 'Straightforward', 'gender': '0', 'birthDate': '2008-09-22T13:57:31.2311892-04:00', 'phoneNumber': '+395887647288', 'languageIds' : ['72f3a2cf-a9ab-4f93-a581-7ae07e812ef4','72f3a2cf-a9ab-4f93-a581-7ae07e812ef1'] }</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult Put(Guid id, [FromBody]UpdateUserInfoRequestDto updateUserData)
        {
            var validator = new UpdatedUserInfoRequestDtoValidator(id, this._phoneValidationManager, this._userService);
            var results = validator.Validate(updateUserData);

            var detailedError = new StringBuilder();
            foreach (var error in results.Errors.Select(e => e.ErrorMessage))
            {
                detailedError.Append(error + "; ");
            }

            if (!results.IsValid)
                return BadRequest(detailedError.ToString()); // TODO: Think of returning the errors in a better way

            try
            {
                this._userService.UpdateUserInfo(id, updateUserData);

                return Ok();
            }
            catch (Exception ex)
            {
                _apiLogger.LogError("Error while updating user.", ex);
                return InternalServerError();
            }
        }

        // TODO: Manually test this!!!
        /// <summary>
        /// Example: GET api/v1/users/{id:Guid}/around/{lat:double}/{lon:double}/subscriptions
        /// </summary>
        /// <param name="id">Id of the user we want to get the subscribtions</param>
        /// <param name="lat">Latitude of the current user location</param>
        /// <param name="lon">Longitude of the current user location</param>
        /// <returns>List of Guids which indicate the group ids to which the user is subscribed and these specific groups are around current user location.</returns>
        [HttpGet]
        [Route("{id}/around/{lat}/{lon}/subscriptions")]
        public IHttpActionResult GetSubscriptions(Guid id, double lat, double lon)
        {
            try
            {
                var result = this._groupService.GetUserSubscriptions(id, lat, lon);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _apiLogger.LogError("Error getting user subscribtions.", ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Example: PUT api/v1/users/{id:Guid}/UpdateAutomaticSubscriptionSettings
        /// </summary>
        /// <param name="id">Unique identifier of the user whose notification settings we are updating</param>
        /// <param name="subscriptionSettings">Body sample: {"automaticallySubscribeToAllGroups" : true, "automaticallySubscribeToAllGroupsWithTag" :  false, "subscribedTagIds" : ['72f3a2cf-a9ab-4f93-a581-7ae07e812ef4', '72f3a2cf-a9ab-4f93-a581-7ae07e812234']}</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id}/UpdateAutomaticSubscriptionSettings")]
        public IHttpActionResult Put(Guid id, [FromBody] EditUserAutomaticSubscriptionSettingsRequestDto subscriptionSettings)
        {
            if (subscriptionSettings.AutomaticallySubscribeToAllGroups && 
                subscriptionSettings.AutomaticallySubscribeToAllGroupsWithTag)
                return BadRequest("The options for AutomaticallySubscribeToAllGroups and AutomaticallySubscribeToAllGroupsWithTag can not be both set to 'true'.");

            try
            {
                this._userService.UpdateUserNotificationSettings(id, subscriptionSettings);

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
                var result = this._userService.GetNotificationSettingsForUserWithId(id);

                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _apiLogger.LogError("Error getting user notification settings.", ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Example: GET api/v1/users/{id:Guid}/UpdateLocation/{lat:double}/{lon:double}/
        /// </summary>
        /// <param name="id">Id of the user that is reporting his latest location.</param>
        /// <param name="lat">Latest user latitude</param>
        /// <param name="lon">Latest user longitude</param>
        /// <returns>SecodsToClosestBoundary: used by the accelometer-based algorithm, SubscribedGroupIds: groups user subscribed when he was here before</returns>
        [HttpGet]
        [Route("{id}/UpdateLocation/{lat}/{lon}")]
        public IHttpActionResult UpdateLocation(Guid id, double lat, double lon)
        {
            if (Math.Abs(lat) > 90 || Math.Abs(lon) > 90)
                return BadRequest("Latitude and longitude values must be in the [-90, 90] range.");

            try
            {
                var result = this._userService.UpdateUserLocation(id, lat, lon);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _apiLogger.LogError("Error when trying to update latest user location.", ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Example: GET api/v1/users/{id:Guid}/SocialLinks
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/SocialLinks")]
        public IHttpActionResult GetSocialLinks(Guid id)
        {
            try
            {
                var result = this._userService.GetSocialLinksForUser(id);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _apiLogger.LogError("Error while trying to get social links for user.", ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Example: POST api/v1/users/{id:Guid}/UpdateSocialLinks
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updatedSocialLinks">Body sample: {'FacebookUsername': 'm.me/smfbuser', 'InstagramUsername': 'krstnznam', 'TwitterUsername': '@stefhano'}</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id}/UpdateSocialLinks")]
        public IHttpActionResult UpdateSocialLinks(Guid id, UpdateSocialLinksRequestDto updatedSocialLinks)
        {
            var validator = new UpdateSocialLinksRequestDtoValidator();
            var results = validator.Validate(updatedSocialLinks);

            var detailedError = new StringBuilder();
            foreach (var error in results.Errors.Select(e => e.ErrorMessage))
            {
                detailedError.Append(error + "; ");
            }

            if (!results.IsValid)
                return BadRequest(detailedError.ToString()); // TODO: Think of returning the errors in a better way

            try
            {
                this._userService.UpdateSocialLinksForUser(id, updatedSocialLinks);

                return Created($"api/v1/users/{id}/SocialLinks", "Success");
            }
            catch (Exception ex)
            {
                _apiLogger.LogError("Error while trying to get social links for user.", ex);
                return InternalServerError();
            }
        }
    }
}
