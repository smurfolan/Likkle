using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessEntities.Responses;
using Likkle.BusinessServices;
using Likkle.WebApi.Owin.Controllers;
using Likkle.WebApi.Owin.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

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
                _apiLogger.Object);

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
                _apiLogger.Object);

            // act
            var actionResult = groupController.Get(Guid.NewGuid());

            // assert
            _apiLogger.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
            Assert.IsInstanceOfType(actionResult, typeof(InternalServerErrorResult));
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
                _apiLogger.Object);

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
                _apiLogger.Object);

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
                _apiLogger.Object);

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

            var newGroupId = Guid.NewGuid();

            mockedDataService.Setup(x => x.InserGroupAsNewArea(It.IsAny<GroupAsNewAreaRequestDto>())).Returns(newGroupId);

            // act
            var groupController = new GroupController(
                mockedDataService.Object,
                _apiLogger.Object);

            var actionResult = await groupController.Post(new GroupAsNewAreaRequestDto());

            var contentResult = actionResult as CreatedNegotiatedContentResult<string>;

            // assert
            Assert.IsNotNull(contentResult);
            Assert.IsTrue(string.CompareOrdinal(contentResult.Location.ToString(), "api/v1/groups/" + newGroupId) == 0);
        }

        [TestMethod]
        public void Getting_Group_By_Coordinates_Exception_Is_Properly_Handled()
        {
            // arrange
            var mockedGroupService = new Mock<IGroupService>();

            var groupController = new GroupController(
                mockedGroupService.Object,
                _apiLogger.Object);

            mockedGroupService.Setup(x => x.GetGroups(It.IsAny<double>(), It.IsAny<double>()))
                .Throws(new Exception("Joke"));

            // act
            var actionResult = groupController.Get(90, 90);

            // assert
            _apiLogger.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
            Assert.IsInstanceOfType(actionResult, typeof(InternalServerErrorResult));
        }

        [TestMethod]
        public void Client_Gets_Bad_Request_If_LatLon_Combination_Is_Not_Valid_When_Requesting_GroupCreationType()
        {
            // arrange
            var mockedDataService = new Mock<IGroupService>();

            var groupController = new GroupController(
                mockedDataService.Object,
                _apiLogger.Object);

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
                _apiLogger.Object);

            // act
            var actionResult = groupController.Get(-91, 23);
            var contentResult = actionResult as BadRequestErrorMessageResult;

            // assert
            Assert.IsNotNull(contentResult);
            Assert.AreEqual("Latitude and longitude values must be in the [-90, 90] range.", contentResult.Message);
            mockedDataService.Verify(m => m.GetGroups(It.IsAny<double>(), It.IsAny<double>()), Times.Never);
        }
    }
}
