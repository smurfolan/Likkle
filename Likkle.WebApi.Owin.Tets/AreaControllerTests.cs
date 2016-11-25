using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Enums;
using Likkle.BusinessEntities.Requests;
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
            var mockedDataService = new Mock<IDataService>();

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
            var mockedDataService = new Mock<IDataService>();
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
            var mockedDataService = new Mock<IDataService>();

            var areaDtoResultId = Guid.NewGuid();

            mockedDataService.Setup(x => x.GetAreas(It.IsAny<double>(), It.IsAny<double>())).Returns(new List<AreaDto>()
            {
                new AreaDto()
                {
                    Id = areaDtoResultId
                }
            });

            var areaController = new AreaController(
                mockedDataService.Object,
                _apiLogger.Object);

            //
            var actionResult = areaController.Get(23, 23);
            var contentResult = actionResult as OkNegotiatedContentResult<IEnumerable<AreaDto>>;

            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(areaDtoResultId, contentResult.Content.FirstOrDefault().Id);
        }

        [TestMethod]
        public void Client_Can_Post_New_Area()
        {
            // arrange
            var mockedDataService = new Mock<IDataService>();

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
            var mockedDataService = new Mock<IDataService>();

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
            throw new NotImplementedException();
        }
    }
}
