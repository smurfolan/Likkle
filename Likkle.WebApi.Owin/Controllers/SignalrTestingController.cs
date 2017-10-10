using System;
using System.Collections.Generic;
using System.Web.Http;
using Likkle.BusinessEntities.Enums;
using Likkle.BusinessEntities.SignalrDtos;
using Likkle.BusinessServices;

namespace Likkle.WebApi.Owin.Controllers
{
    /// <summary>
    /// Testing purposes controller. It is used to fire an event on SignalR which afects some of the connected users.
    /// In the specific case Kevin and Bubka.
    /// </summary>
    [RoutePrefix("api/v1/signalrtesting")]
    public class SignalrTestingController : ApiController
    {
        private readonly ISignalrService _signalrService;

        public SignalrTestingController(ISignalrService signalrService)
        {
            _signalrService = signalrService;
        }

        [HttpGet]
        [Route("groupAroundMeWasRecreated/{groupId}")]
        public IHttpActionResult GetResultFromGroupAroundMeWasRecreated(string groupId)
        {
            try
            {
                this._signalrService.GroupAroundMeWasRecreated(
                    groupId, 
                    new List<SRAreaDto>()
                    {
                        new SRAreaDto()
                        {
                            ApproximateAddress = "Random approximate address",
                            GroupIds = new List<Guid>() {Guid.NewGuid(), Guid.NewGuid() },
                            Id = Guid.NewGuid(),
                            Longitude = 10.10,
                            Latitude = 11.11,
                            Radius = RadiusRangeEnum.FiftyMeters
                        },
                        new SRAreaDto()
                        {
                            ApproximateAddress = "Random approximate address 2",
                            GroupIds = new List<Guid>() {Guid.NewGuid(), Guid.NewGuid() },
                            Id = Guid.NewGuid(),
                            Longitude = 45.10,
                            Latitude = 33.11,
                            Radius = RadiusRangeEnum.FiveHundredMeters
                        }
                    }, 
                    new SRGroupDto()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Random group name",
                        TagIds = new List<Guid>() { Guid.Parse("0c53eeff-06a1-4104-a86e-1bd3c8028a00"), Guid.Parse("ca97f757-f249-4d7d-ac80-814498348688") },
                        UserIds = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid()}
                    }, 
                    false
                );
                
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("groupAsNewAreaWasCreatedAroundMe/{groupId}")]
        public IHttpActionResult GetResultFromGroupAsNewAreaWasCreatedAroundMe(string groupId)
        {
            try
            {
                this._signalrService.GroupAsNewAreaWasCreatedAroundMe(
                    groupId,
                    new SRAreaDto()
                    {
                        ApproximateAddress = "Random approximate address",
                        GroupIds = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() },
                        Id = Guid.NewGuid(),
                        Longitude = 10.10,
                        Latitude = 11.11,
                        Radius = RadiusRangeEnum.FiftyMeters
                    }, 
                    new SRGroupDto()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Random group name",
                        TagIds = new List<Guid>() { Guid.Parse("0c53eeff-06a1-4104-a86e-1bd3c8028a00"), Guid.Parse("ca97f757-f249-4d7d-ac80-814498348688") },
                        UserIds = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() }
                    }, 
                    false);

                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("groupAttachedToExistingAreasWasCreatedAroundMe/{groupId}")]
        public IHttpActionResult GetResultFromGroupAttachedToExistingAreasWasCreatedAroundMe(string groupId)
        {
            try
            {
                this._signalrService.GroupAttachedToExistingAreasWasCreatedAroundMe(
                    groupId,
                    new List<Guid>() {Guid.NewGuid(), Guid.NewGuid()},
                    new SRGroupDto() {
                        Id = Guid.NewGuid(),
                        Name = "Random group name",
                        TagIds = new List<Guid>() { Guid.Parse("0c53eeff-06a1-4104-a86e-1bd3c8028a00"), Guid.Parse("ca97f757-f249-4d7d-ac80-814498348688") },
                        UserIds = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() }
                    },
                    false);

                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("groupWasJoinedByUser")]
        public IHttpActionResult GetResultFromGroupWasJoinedByUser()
        {
            try
            {
                this._signalrService.GroupWasJoinedByUser(Guid.NewGuid(), new List<string>()
                {
                    "5b8e69b6-fc13-494d-9228-4215de85254f"/*Kevin*/,
                    "b535df55-6a8e-4ef5-aecc-71797cabbda5"/*Bubka*/
                });

                return Ok();
            }
            catch(Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("groupWasLeftByUser")]
        public IHttpActionResult GetResultFromGroupWasLeftByUser()
        {
            try
            {
                this._signalrService.GroupWasLeftByUser(Guid.NewGuid(), new List<string>()
                {
                    "5b8e69b6-fc13-494d-9228-4215de85254f"/*Kevin*/,
                    "b535df55-6a8e-4ef5-aecc-71797cabbda5"/*Bubka*/
                });

                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
