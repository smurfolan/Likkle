using System;
using System.Collections.Generic;
using System.Web.Http.Results;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessServices;
using Likkle.BusinessServices.Utils;
using Likkle.WebApi.Owin.Controllers;
using Likkle.WebApi.Owin.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Likkle.WebApi.Owin.Tets
{
    [TestClass]
    public class UserControllerTests
    {
        private readonly Mock<ILikkleApiLogger> _apiLogger;

        public UserControllerTests()
        {
            this._apiLogger = new Mock<ILikkleApiLogger>();
        }

        [TestMethod]
        public void Bad_Request_Is_Returned_When_Phone_Number_Invalid()
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
            var actionResult = userController.Post(request);

            // assert
            var contentResult = actionResult as BadRequestErrorMessageResult;
            Assert.IsNotNull(contentResult);
            Assert.AreEqual("'First Name' must not be empty.; Phone number provided is invalid; User with the same STS id has been already added.; User with the same email has been already added.; ", contentResult.Message);
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
            Assert.AreEqual("The options for AutomaticallySubscribeToAllGroups and AutomaticallySubscribeToAllGroupsWithTag can not be both set to 'true'.", contentResult.Message);
        }
    }
}
