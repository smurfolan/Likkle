using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web.Http.Results;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessServices;
using Likkle.BusinessServices.Utils;
using Likkle.WebApi.Owin.Controllers;
using Likkle.WebApi.Owin.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Likkle.WebApi.Owin.Tets
{
    [TestClass]
    public class UserControllerTests
    {
        private readonly Mock<ILikkleApiLogger> _apiLogger;
        private readonly Mock<IUserService> _mockedUserService;

        public UserControllerTests()
        {
            this._apiLogger = new Mock<ILikkleApiLogger>();
            this._mockedUserService = new Mock<IUserService>();

            this._mockedUserService.Setup(us => us.GetAllUsers()).Returns(new List<UserDto>()
            {
                new UserDto
                {
                    Email = "Some@mail.com"
                }
            });
        }

        [TestMethod]
        public async Task Bad_Request_Is_Returned_When_Phone_Number_Invalid()
        {
            // arrange
            var phoneValidationManagerMock = new Mock<IPhoneValidationManager>();
            var userServiceMock = new Mock<IUserService>();

            userServiceMock.Setup(us => us.GetAllUsers()).Returns(new List<UserDto>()
            {
                new UserDto()
                {
                    IdsrvUniqueId = "idsrvid",
                    Email = "mail@mail.com",
                    PhoneNumber = "+359883899902"
                }
            });

            var request = new NewUserRequestDto()
            {
                IdsrvUniqueId = "idsrvid",
                Email = "mail@mail.com",
                PhoneNumber = "+359883899902"
            };

            var userController = new UserController(
                userServiceMock.Object,
                this._apiLogger.Object,
                null,
                null,
                phoneValidationManagerMock.Object);

            // act
            var actionResult = await userController.Post(request);

            // assert
            var contentResult = actionResult as BadRequestErrorMessageResult;
            Assert.IsNotNull(contentResult);
            Assert.AreEqual(
                "'First Name' must not be empty.; Phone number provided is invalid; User with the same STS id has been already added.; User with the same email has been already added.; ",
                contentResult.Message);
        }

        [TestMethod]
        public void Bad_Request_Is_Returned_When_AutomaicSubscriptionSettings_Are_Not_Self_Excluding()
        {
            // arrange
            var request = new EditUserAutomaticSubscriptionSettingsRequestDto()
            {
                AutomaticallySubscribeToAllGroups = true,
                AutomaticallySubscribeToAllGroupsWithTag = true
            };

            var userController = new UserController(null, null, null, null, null);

            // act
            var actionResult = userController.Put(Guid.NewGuid(), request);
            var contentResult = actionResult as BadRequestErrorMessageResult;

            // assert
            Assert.IsNotNull(contentResult);
            Assert.AreEqual(
                "The options for AutomaticallySubscribeToAllGroups and AutomaticallySubscribeToAllGroupsWithTag can not be both set to 'true'.",
                contentResult.Message);
        }

        [TestMethod]
        public void User_Latest_Coordinates_Are_Updated_When_User_Updates_His_Location()
        {
            // arrange
            var userServiceMock = new Mock<IUserService>();
            var subscriptionServiceMock = new Mock<ISubscriptionService>();

            userServiceMock.Setup(us => us.UpdateUserLocation(It.IsAny<Guid>(), It.IsAny<double>(), It.IsAny<double>()));
            subscriptionServiceMock.Setup(
                ss =>
                    ss.UpdateLatestWellKnownUserLocation(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<IPrincipal>()));

            var userController = new UserController(userServiceMock.Object, null, subscriptionServiceMock.Object, null,
                null);

            // act
            var actionResult = userController.UpdateLocation(Guid.NewGuid(), 22, 33);

            // assert
            subscriptionServiceMock.Verify(
                usm =>
                    usm.UpdateLatestWellKnownUserLocation(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<IPrincipal>()),
                Times.Once);
        }

        [TestMethod]
        public void User_Latest_Coordinates_Are_Updated_When_Getting_Subscriptions_Around_Coordinates()
        {
            // arrange
            var groupServiceMock = new Mock<IGroupService>();
            var subscriptionServiceMock = new Mock<ISubscriptionService>();

            groupServiceMock.Setup(
                us => us.GetUserSubscriptions(It.IsAny<Guid>(), It.IsAny<double>(), It.IsAny<double>()));
            subscriptionServiceMock.Setup(
                ss =>
                    ss.UpdateLatestWellKnownUserLocation(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<IPrincipal>()));

            var userController = new UserController(null, null, subscriptionServiceMock.Object, groupServiceMock.Object,
                null);

            // act
            var actionResult = userController.GetSubscriptions(Guid.NewGuid(), 22, 33);

            // assert
            subscriptionServiceMock.Verify(
                usm =>
                    usm.UpdateLatestWellKnownUserLocation(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<IPrincipal>()),
                Times.Once);
        }

        [TestMethod]
        public void User_Latest_Coordinates_Are_Updated_When_Changing_Group_Subscriptions()
        {
            // arrange
            var subscriptionServiceMock = new Mock<ISubscriptionService>();

            subscriptionServiceMock.Setup(ss => ss.RelateUserToGroups(It.IsAny<RelateUserToGroupsDto>()));

            subscriptionServiceMock.Setup(
                ss =>
                    ss.UpdateLatestWellKnownUserLocation(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<IPrincipal>()));

            var userController = new UserController(null, null, subscriptionServiceMock.Object, null, null);

            // act
            var actionResult = userController.Post(new RelateUserToGroupsDto() {GroupsUserSubscribes = new List<Guid>(), Longitude = 23, Latitude = 33, UserId = Guid.NewGuid()});

            // assert
            subscriptionServiceMock.Verify(
                usm =>
                    usm.UpdateLatestWellKnownUserLocation(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<IPrincipal>()),
                Times.Once);
        }
    }
}
