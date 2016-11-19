﻿using System;
using System.Web.Http;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessServices;
using Likkle.WebApi.Owin.Helpers;

namespace Likkle.WebApi.Owin.Controllers
{
    [Authorize]
    [RoutePrefix("api/v1/groups")]
    public class GroupController : ApiController
    {
        private readonly IDataService _likkleDataService;

        public GroupController(IDataService dataService)
        {
            this._likkleDataService = dataService;
        }

        /// <summary>
        /// Example: GET /api/v1/groups/{id:Guid}
        /// </summary>
        /// <param name="id">Unique identifier of a group</param>
        /// <returns>Specific group by its id.</returns>
        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult Get(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = this._likkleDataService.GetGroupById(id);

                return Ok(result);
            }
            catch (Exception ex)
            {
                LikkleApiLogger.LogError("Error while getting group by id.", ex);
                return InternalServerError();
            }  
        }

        /// <summary>
        /// Example: GET /api/v1/groups/{lat:double}/{lon:double}/
        /// </summary>
        /// <param name="lat">Latitude</param>
        /// <param name="lon">Longitude</param>
        /// <returns>All the groups that contain this point(lat/lon) as part of their diameter</returns>
        [HttpGet]
        [Route("{lat:double}/{lon:double}")]
        public IHttpActionResult Get(double lat, double lon)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = this._likkleDataService.GetGroups(lat, lon);

                return Ok(result);
            }
            catch (Exception ex)
            {
                LikkleApiLogger.LogError("Error while getting groups around coordinates.", ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Example: GET api/v1/groups/{id:Guid}/users
        /// </summary>
        /// <param name="id">Unique identifier of the group you are getting the users from</param>
        /// <returns>All the users for a specific group</returns>
        [HttpGet]
        [Route("{id}/users")]
        public IHttpActionResult GetUsers(Guid id)
        {
            try
            {
                var result = this._likkleDataService.GetUsersFromGroup(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                LikkleApiLogger.LogError("Error while getting users for group.", ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Example: POST /api/v1/groups
        /// </summary>
        /// <param name="newGroup">Body sample:{'name':'Second floor cooks', 'tagIds':['0c53eeff-06a1-4104-a86e-1bd3c8028a00', 'afc3c12f-b884-40e2-b356-2c863fd0b86c'], 'areaIds':['c6f22434-fbc1-47f5-8149-2dd57f78a29e'],'userId':'9fa631dd-7d0d-4235-b330-baf23862d90b'}</param>
        /// <returns>HTTP Code 201 if successfuly created and 500 if not.</returns>
        [HttpPost]
        [Route("")]
        public IHttpActionResult Post([FromBody] StandaloneGroupRequestDto newGroup)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var newGroupId = this._likkleDataService.InsertNewGroup(newGroup);
                return Created("api/v1/groups/" + newGroupId, "Success");
            }
            catch (Exception ex)
            {
                LikkleApiLogger.LogError("Error while inserting a new group.", ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// Example: POST /api/v1/groups/AsNewArea
        /// </summary>
        /// <param name="newGroup">Body sample:{'name':'Second floor cooks', 'tagIds':['fd463953-118b-434a-9c76-d11d4366d742', '92b93aac-97b4-461b-80f7-ddd6c6b8ed7f'], 'userId':'92b93aac-97b4-461b-80f7-ddd6c6b8ed22', 'latitude':42.657064, 'longitude':23.28539, 'radius':50}</param>
        /// <param name="AreaIdsNote">NOTE: 'AreaIds':[1] -> NOT Required. This is not the Id of the newly created area. This is all the other area ids if we were in the range of other areas but no matter of that we decided to create new area.</param>
        /// <param name="UserIdNote">NOTE: 'userId':1 -> Supposed to be the id of the user. If you pass it, you automatically get subscribed to the group you created. Otherwise, you just create it without following it.</param>
        /// <returns>Uniqe identifier of the newly created group entity</returns>
        [HttpPost]
        [Route("AsNewArea")]
        public IHttpActionResult Post([FromBody] GroupAsNewAreaRequestDto newGroup)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var newlyCreatedGroupId = this._likkleDataService.InserGroupAsNewArea(newGroup);

                return Created("api/v1/groups/" + newlyCreatedGroupId, "Success");
            }
            catch (Exception ex)
            {
                LikkleApiLogger.LogError("Error while creating new group.", ex);
                return InternalServerError();
            }
        }
    }
}
