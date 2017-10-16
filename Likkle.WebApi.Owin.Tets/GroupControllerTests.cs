using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web.Http.Results;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessEntities.Responses;
using Likkle.BusinessServices;
using Likkle.WebApi.Owin.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Likkle.BusinessServices.Utils;

namespace Likkle.WebApi.Owin.Tets
{
    [TestClass]
    public class GroupControllerTests
    {
        private readonly Mock<ILikkleApiLogger> _apiLogger;

        public GroupControllerTests()
        {
            this._apiLogger = new Mock<ILikkleApiLogger>();
        }

        [TestMethod]
        public void Client_Can_Get_Group_By_Id()
        {
            // arrange
            var mockedDataService = new Mock<IGroupService>();

            var groupDtoResultId = Guid.NewGuid();

            mockedDataService.Setup(x => x.GetGroupById(It.IsAny<Guid>())).Returns(new GroupDto()
            {
                Id = groupDtoResultId
            });

            var groupController = new GroupController(
                mockedDataService.Object,
                _apiLogger.Object,
                null, null);

            // act
            var actionResult = groupController.Get(Guid.NewGuid());
            var contentResult = actionResult as OkNegotiatedContentResult<GroupDto>;

            // asssert
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(groupDtoResultId, contentResult.Content.Id);
        }

        [TestMethod]
        public void Client_Gets_500_Error_When_DataService_Throws_Exception_And_Then_Error_Is_Logged()
        {
            // arrange
            var mockedDataService = new Mock<IGroupService>();
            mockedDataService.Setup(x => x.GetGroupById(It.IsAny<Guid>()))
                .Throws(new Exception("Error while getting group from data service."));

            var groupController = new GroupController(
                mockedDataService.Object,
                _apiLogger.Object,
                null, 
                null);

            // act
            var actionResult = groupController.Get(Guid.NewGuid());

            // assert
            _apiLogger.Verify(x => x.OnActionException(It.IsAny<HttpActionContext>(), It.IsAny<Exception>()), Times.Once);
            Assert.IsInstanceOfType(actionResult, typeof(ExceptionResult));
        }

        [TestMethod]
        public void Client_Can_Get_Groups_By_Lat_Lon()
        {
            // arrange
            var mockedDataService = new Mock<IGroupService>();

            var groupDtoResultId = Guid.NewGuid();

            mockedDataService.Setup(x => x.GetGroups(It.IsAny<double>(), It.IsAny<double>())).Returns(new List<GroupMetadataResponseDto>()
            {
                new GroupMetadataResponseDto()
                {
                    Id = groupDtoResultId
                }
            });

            var groupController = new GroupController(
                mockedDataService.Object,
                _apiLogger.Object,
                null,
                null);

            // act
            var actionResult = groupController.Get(23, 23);
            var contentResult = actionResult as OkNegotiatedContentResult<IEnumerable<GroupMetadataResponseDto>>;


            // assert
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(groupDtoResultId, contentResult.Content.FirstOrDefault().Id);
        }

        [TestMethod]
        public void Client_Can_Get_Users_For_A_Group()
        {
            // arrange
            // arrange
            var mockedDataService = new Mock<IGroupService>();

            var userDtoResultId = Guid.NewGuid();

            mockedDataService.Setup(x => x.GetUsersFromGroup(It.IsAny<Guid>())).Returns(new List<UserDto>()
            {
                new UserDto()
                {
                    Id = userDtoResultId
                }
            });

            var groupController = new GroupController(
                mockedDataService.Object,
                _apiLogger.Object,
                null, 
                null);

            // act
            var actionResult = groupController.GetUsers(Guid.NewGuid());
            var contentResult = actionResult as OkNegotiatedContentResult<IEnumerable<UserDto>>;

            // assert
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(userDtoResultId, contentResult.Content.FirstOrDefault().Id);
        }

        [TestMethod]
        public void Client_Can_Post_New_Group()
        {
            // arrange
            var mockedDataService = new Mock<IGroupService>();

            var newGroupId = Guid.NewGuid();

            mockedDataService.Setup(x => x.InsertNewGroup(It.IsAny<StandaloneGroupRequestDto>())).Returns(newGroupId);

            // act
            var groupController = new GroupController(
                mockedDataService.Object,
                _apiLogger.Object,
                null,
                null);

            var actionResult = groupController.Post(new StandaloneGroupRequestDto());

            var contentResult = actionResult as CreatedNegotiatedContentResult<string>;

            // assert
            Assert.IsNotNull(contentResult);
            Assert.IsTrue(string.CompareOrdinal(contentResult.Location.ToString(), "api/v1/groups/" + newGroupId) == 0);
        }

        [TestMethod]
        public async Task Client_Can_Post_New_Group_As_New_AreaAsync()
        {
            // arrange
            var mockedDataService = new Mock<IGroupService>();
            var mockedSubscriptionService = new Mock<ISubscriptionService>();
            var mockedAreaService = new Mock<IAreaService>();

            var newGroupId = Guid.NewGuid();

            mockedSubscriptionService.Setup(
                ss =>
                    ss.UpdateLatestWellKnownUserLocation(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<IPrincipal>()));
            mockedDataService.Setup(x => x.InserGroupAsNewArea(It.IsAny<GroupAsNewAreaRequestDto>())).Returns(newGroupId);

            // act
            var groupController = new GroupController(
                mockedDataService.Object,
                _apiLogger.Object,
                mockedSubscriptionService.Object,
                mockedAreaService.Object);

            var actionResult = await groupController.Post(new GroupAsNewAreaRequestDto());

            var contentResult = actionResult as CreatedNegotiatedContentResult<string>;

            // assert
            Assert.IsNotNull(contentResult);
            Assert.IsTrue(string.CompareOrdinal(contentResult.Location.ToString(), "api/v1/groups/" + newGroupId) == 0);
        }

        [TestMethod]
        public void Client_Gets_Bad_Request_If_LatLon_Combination_Is_Not_Valid_When_Requesting_GroupCreationType()
        {
            // arrange
            var mockedDataService = new Mock<IGroupService>();

            var groupController = new GroupController(
                mockedDataService.Object,
                _apiLogger.Object,
                null,
                null);

            // act
            var actionResult = groupController.GetGroupCreationType(-91, 23, Guid.NewGuid());
            var contentResult = actionResult as BadRequestErrorMessageResult;

            // assert
            Assert.IsNotNull(contentResult);
            Assert.AreEqual("Latitude and longitude values must be in the [-90, 90] range.", contentResult.Message);
            mockedDataService.Verify(m => m.GetGroupCreationType(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<Guid>()), Times.Never);
        }

        [TestMethod]
        public void Client_Gets_Bad_Request_If_LatLon_Combination_Is_Not_Valid_When_Requesting_All_Groups_Around_Coords()
        {
            // arrange
            var mockedDataService = new Mock<IGroupService>();

            var groupController = new GroupController(
                mockedDataService.Object,
                _apiLogger.Object,
                null,
                null);

            // act
            var actionResult = groupController.Get(-91, 23);
            var contentResult = actionResult as BadRequestErrorMessageResult;

            // assert
            Assert.IsNotNull(contentResult);
            Assert.AreEqual("Latitude and longitude values must be in the [-90, 90] range.", contentResult.Message);
            mockedDataService.Verify(m => m.GetGroups(It.IsAny<double>(), It.IsAny<double>()), Times.Never);
        }

        [TestMethod]
        public void User_Latest_Coordinates_Are_Updated_When_Getting_GroupCreationType()
        {
            // arrange
            var subscriptionServiceMock = new Mock<ISubscriptionService>();
            var groupServiceMock = new Mock<IGroupService>();
            var mockedAreaService = new Mock<IAreaService>();

            groupServiceMock.Setup(
                gs => gs.GetGroupCreationType(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<Guid>())).Returns(new PreGroupCreationResponseDto());

            subscriptionServiceMock.Setup(
                ss =>
                    ss.UpdateLatestWellKnownUserLocation(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<IPrincipal>()));

            var groupController = new GroupController(
                groupServiceMock.Object, 
                null, 
                subscriptionServiceMock.Object,
                mockedAreaService.Object);

            // act
            var actionResult = groupController.GetGroupCreationType(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<Guid>());

            // assert
            subscriptionServiceMock.Verify(
                usm =>
                    usm.UpdateLatestWellKnownUserLocation(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<IPrincipal>()),
                Times.Once);
        }

        [TestMethod]
        public void User_Latest_Coordinates_Are_Updated_When_Posting_Group_As_New_Area()
        {
            // arrange
            var subscriptionServiceMock = new Mock<ISubscriptionService>();
            var groupServiceMock = new Mock<IGroupService>();
            var mockedAreaService = new Mock<IAreaService>();

            groupServiceMock.Setup(
                gs => gs.InserGroupAsNewArea(It.IsAny<GroupAsNewAreaRequestDto>())).Returns(Guid.NewGuid);

            subscriptionServiceMock.Setup(
                ss =>
                    ss.UpdateLatestWellKnownUserLocation(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<IPrincipal>()));

            var groupController = new GroupController(
                groupServiceMock.Object, 
                null, 
                subscriptionServiceMock.Object,
                mockedAreaService.Object);

            // act
            var actionResult = groupController.Post(new GroupAsNewAreaRequestDto() {});

            // assert
            subscriptionServiceMock.Verify(
                usm =>
                    usm.UpdateLatestWellKnownUserLocation(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<IPrincipal>()),
                Times.Once);
        }

        [TestMethod]
        public void GroupAsNewAre_Can_Not_Be_Posted_Twice_With_Same_Coordinates()
        {
            // arrange
            var subscriptionServiceMock = new Mock<ISubscriptionService>();
            var groupServiceMock = new Mock<IGroupService>();
            var mockedAreaService = new Mock<IAreaService>();

            mockedAreaService.Setup(ars => ars.GetAllAreas()).Returns(new List<AreaDto>() {
                new AreaDto()
                {
                    Longitude = 10, Latitude = 10, Radius = BusinessEntities.Enums.RadiusRangeEnum.FiftyMeters
                }
            });

            var request = new GroupAsNewAreaRequestDto()
            {
                Latitude = 10,
                Longitude = 10,
                Radius = BusinessEntities.Enums.RadiusRangeEnum.FiftyMeters
            };

            var groupController = new GroupController(
                groupServiceMock.Object,
                null,
                subscriptionServiceMock.Object,
                mockedAreaService.Object);

            // act
            var actionResult = groupController.Post(request).Result;
            var contentResult = actionResult as BadRequestErrorMessageResult;

            // assert
            Assert.IsNotNull(contentResult);
            Assert.AreEqual("There's a previous request with these coordinates and radius; ", contentResult.Message);
        }
    }
}
