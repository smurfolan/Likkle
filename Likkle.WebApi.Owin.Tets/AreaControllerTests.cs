using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Enums;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessEntities.Responses;
using Likkle.BusinessServices;
using Likkle.WebApi.Owin.Controllers;
using Likkle.WebApi.Owin.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Likkle.WebApi.Owin.Tets
{
    [TestClass]
    public class AreaControllerTests
    {
        private readonly Mock<ILikkleApiLogger> _apiLogger;

        public AreaControllerTests()
        {
            this._apiLogger = new Mock<ILikkleApiLogger>();
        }

        [TestMethod]
        public void Client_Can_Get_Area_By_Id()
        {
            // arrange
            var mockedDataService = new Mock<IAreaService>();

            var areaDtoResultId = Guid.NewGuid();

            mockedDataService.Setup(x => x.GetAreaById(It.IsAny<Guid>())).Returns(new AreaDto()
            {
                Id = areaDtoResultId
            });

            var areaController = new AreaController(
                mockedDataService.Object, 
                _apiLogger.Object);

            // act
            var actionResult = areaController.Get(Guid.NewGuid());
            var contentResult = actionResult as OkNegotiatedContentResult<AreaDto>;

            // assert
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(areaDtoResultId, contentResult.Content.Id);
        }

        [TestMethod]
        public void Client_Gets_500_Error_When_DataService_Throws_Exception_And_Then_Error_Is_Logged()
        {
            // arrange
            var mockedDataService = new Mock<IAreaService>();
            mockedDataService.Setup(x => x.GetAreaById(It.IsAny<Guid>()))
                .Throws(new Exception("Error while getting area from data service."));

            var areaController = new AreaController(
                mockedDataService.Object, 
                _apiLogger.Object);

            // act
            var actionResult = areaController.Get(Guid.NewGuid());

            // assert
            _apiLogger.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
            Assert.IsInstanceOfType(actionResult, typeof(InternalServerErrorResult)); 
        }

        [TestMethod]
        public void Client_Can_Get_Area_By_Lat_Lon()
        {
            // arrange
            var mockedDataService = new Mock<IAreaService>();

            var areaDtoResultId = Guid.NewGuid();

            mockedDataService.Setup(x => x.GetAreas(It.IsAny<double>(), It.IsAny<double>())).Returns(new List<AreaForLocationResponseDto>()
            {
                new AreaForLocationResponseDto()
                {
                    Id = areaDtoResultId
                }
            });

            var areaController = new AreaController(
                mockedDataService.Object,
                _apiLogger.Object);

            //
            var actionResult = areaController.Get(23, 23);
            var contentResult = actionResult as OkNegotiatedContentResult<IEnumerable<AreaForLocationResponseDto>>;

            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(areaDtoResultId, contentResult.Content.FirstOrDefault().Id);
        }

        [TestMethod]
        public void Client_Can_Post_New_Area()
        {
            // arrange
            var mockedDataService = new Mock<IAreaService>();

            var newAreaId = Guid.NewGuid();

            mockedDataService.Setup(x => x.InsertNewArea(It.IsAny<NewAreaRequest>())).Returns(newAreaId);

            // act
            var areaController = new AreaController(
                mockedDataService.Object,
                _apiLogger.Object);

            var actionResult = areaController.Post(new NewAreaRequest()
            {
                Latitude = 1,
                Longitude = 1,
                Radius = RadiusRangeEnum.FiftyMeters
            });

            var contentResult = actionResult as CreatedNegotiatedContentResult<string>;

            // assert
            Assert.IsNotNull(contentResult);
            Assert.IsTrue(string.CompareOrdinal(contentResult.Location.ToString(), "api/v1/areas/" + newAreaId) == 0 );
        }

        [TestMethod]
        public void Client_Gets_NotFound_Response_If_Entity_Not_Available()
        {
            // arrange
            var mockedDataService = new Mock<IAreaService>();

            mockedDataService.Setup(x => x.GetAreaById(It.IsAny<Guid>())).Returns((AreaDto) null);

            var areaController = new AreaController(
                mockedDataService.Object,
                _apiLogger.Object);

            // act
            var actionResult = areaController.Get(Guid.NewGuid());
            var contentResult = actionResult as NotFoundResult;

            // assert
            Assert.IsNotNull(contentResult);
            Assert.IsInstanceOfType(contentResult, typeof(NotFoundResult));
        }

        [TestMethod]
        public void Client_Gets_AreaMetadata()
        {
            // arrange
            var mockedDataService = new Mock<IAreaService>();

            mockedDataService.Setup(x => x.GetMetadataForArea(
                It.IsAny<double>(), 
                It.IsAny<double>(), 
                It.IsAny<Guid>())).Returns(new AreaMetadataResponseDto());

            var areaController = new AreaController(
                mockedDataService.Object,
                _apiLogger.Object);

            // act
            var actionResult = areaController.GetAreaMetadata(-91, 91, Guid.NewGuid());
            var contentResult = actionResult as BadRequestErrorMessageResult;

            // assert
            Assert.IsNotNull(contentResult);
            Assert.AreEqual(contentResult.Message, "Latitude and longitude values must be in the [-90, 90] range.");
            Assert.IsInstanceOfType(contentResult, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public void Client_Gets_Multiple_Areas_Metadata()
        {
            // arrange
            var mockedDataService = new Mock<IAreaService>();

            mockedDataService.Setup(x => x.GetMultipleAreasMetadata(It.IsAny<MultipleAreasMetadataRequestDto>()))
                .Returns(new List<AreaMetadataResponseDto>()
                {
                    new AreaMetadataResponseDto()
                    {
                        DistanceTo = 10,
                        NumberOfParticipants = 2,
                        TagIds = new List<Guid>()
                    }
                });

            var areaController = new AreaController(
                mockedDataService.Object,
                _apiLogger.Object);

            // act
            var actionResult = areaController.GetMultipleAreasMetadata(new MultipleAreasMetadataRequestDto());
            var contentResult = actionResult as OkNegotiatedContentResult<IEnumerable<AreaMetadataResponseDto>>;

            // assert
            Assert.IsNotNull(contentResult);

            // arrange
            mockedDataService.Setup(x => x.GetMultipleAreasMetadata(It.IsAny<MultipleAreasMetadataRequestDto>()))
                .Returns((IEnumerable<AreaMetadataResponseDto>) null);

            areaController = new AreaController(
                mockedDataService.Object,
                _apiLogger.Object);

            // act
            actionResult = areaController.GetMultipleAreasMetadata(new MultipleAreasMetadataRequestDto());

            // assert
            _apiLogger.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
            Assert.IsInstanceOfType(actionResult, typeof(InternalServerErrorResult));
        }

        [TestMethod]
        public void We_Can_Not_Insert_New_Area_With_Same_Center_And_Radius()
        {
            // arrange
            var mockedAreaService = new Mock<IAreaService>();

            mockedAreaService.Setup(a => a.GetAllAreas()).Returns(new List<AreaDto>()
            {
                new AreaDto()
                {
                    Latitude = 12.121212,
                    Longitude = 12.121212,
                    Radius = RadiusRangeEnum.FiftyMeters,
                    Id = Guid.NewGuid()
                }
            });

            var areaController = new AreaController(
                mockedAreaService.Object,
                _apiLogger.Object);

            // act
            var actionResult = areaController.Post(new NewAreaRequest()
            {
                Latitude = 12.121212,
                Longitude = 12.121212,
                Radius = RadiusRangeEnum.FiftyMeters
            });

            // assert
            var contentResult = actionResult as BadRequestErrorMessageResult;
            Assert.IsNotNull(contentResult);
            Assert.AreEqual("There's a previous request with these coordinates and radius; ", contentResult.Message);
        }
    }
}
