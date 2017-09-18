﻿using System;
using System.Collections.Generic;
using System.Web.Http;
using Likkle.BusinessEntities.Enums;
using Likkle.BusinessEntities.SignalrDtos;
using Likkle.BusinessServices;

namespace Likkle.WebApi.Owin.Controllers
{
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
                        TagIds = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid()},
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
                        TagIds = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() },
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
                        TagIds = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() },
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
    }
}