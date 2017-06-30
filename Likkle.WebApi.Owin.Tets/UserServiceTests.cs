using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Enums;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessServices;
using Likkle.DataModel;
using Likkle.DataModel.Repositories;
using Likkle.DataModel.TestingPurposes;
using Likkle.DataModel.UnitOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Likkle.WebApi.Owin.Tets
{
    [TestClass]
    public class UserServiceTests
    {
        private readonly string InitialDateString = "01/01/1753";

        private readonly IUserService _userService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly ISubscriptionSettingsService _subscriptionSettingsService;

        private readonly Mock<ILikkleUoW> _mockedLikkleUoW;
        private readonly Mock<IConfigurationProvider> _mockedConfigurationProvider;
        private readonly Mock<IConfigurationWrapper> _configurationWrapperMock;
        private readonly Mock<IAccelometerAlgorithmHelperService> _accelometerAlgorithmHelperService;
        private readonly Mock<ISubscriptionSettingsService> _subscriptionSettingsServiceMock;

        public UserServiceTests()
        {
            var fakeDbContext = new FakeLikkleDbContext().Seed();

            this._mockedLikkleUoW = new Mock<ILikkleUoW>();

            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository)
                .Returns(new AreaRepository(fakeDbContext));

            this._mockedLikkleUoW.Setup(uow => uow.UserRepository)
                .Returns(new UserRepository(fakeDbContext));

            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository)
                .Returns(new GroupRepository(fakeDbContext));

            this._mockedLikkleUoW.Setup(uow => uow.TagRepository)
                .Returns(new TagRepository(fakeDbContext));

            this._mockedLikkleUoW.Setup(uow => uow.LanguageRepository)
                .Returns(new LanguageRepository(fakeDbContext));

            this._accelometerAlgorithmHelperService = new Mock<IAccelometerAlgorithmHelperService>();
            this._accelometerAlgorithmHelperService.Setup(
                a => a.SecondsToClosestBoundary(It.IsAny<double>(), It.IsAny<double>())).Returns(32.5);

            this._subscriptionSettingsServiceMock = new Mock<ISubscriptionSettingsService>();

            this._mockedConfigurationProvider = new Mock<IConfigurationProvider>();

            var mapConfiguration = new MapperConfiguration(cfg => {
                cfg.AddProfile<EntitiesMappingProfile>();
            });
            this._mockedConfigurationProvider.Setup(mc => mc.CreateMapper()).Returns(mapConfiguration.CreateMapper);

            this._subscriptionSettingsService = new SubscriptionSettingsService(
                this._mockedLikkleUoW.Object,
                this._mockedConfigurationProvider.Object);

            this._configurationWrapperMock = new Mock<IConfigurationWrapper>();

            // Only this call to the mocked service returns the actual result but not mocked one.
            this._subscriptionSettingsServiceMock
                .Setup(ss => ss.GroupsForUserAroundCoordinatesBasedOnUserSettings(It.IsAny<Guid>(), It.IsAny<double>(), It.IsAny<double>()))
                .Returns((Guid userId, double lat, double lon) => this._subscriptionSettingsService.GroupsForUserAroundCoordinatesBasedOnUserSettings(userId, lat, lon));
                

            this._userService = new UserService(
                this._mockedLikkleUoW.Object,
                this._mockedConfigurationProvider.Object,
                this._configurationWrapperMock.Object,
                this._accelometerAlgorithmHelperService.Object,
                this._subscriptionSettingsServiceMock.Object);

            this._subscriptionService = new SubscriptionService(
                this._mockedLikkleUoW.Object, 
                this._mockedConfigurationProvider.Object, 
                this._configurationWrapperMock.Object);
        }

        [TestMethod]
        public void We_Can_Create_New_User_Without_Groups_And_Languages()
        {
            // arrange
            var newUserStsId = "https://boongaloocompanysts/identity3025f46b-3070-4f75-809d-44b7ae5b8e6a";

            var newUserRequest = new NewUserRequestDto()
            {
                About = "About",
                Email = "some@body.com",
                FirstName = "Stecho",
                Gender = GenderEnum.Male,
                IdsrvUniqueId = newUserStsId,
                PhoneNumber = "+359886585549"
            };

            // act
            var newUserId = this._userService.InsertNewUser(newUserRequest);

            // assert
            var newUser = this._userService.GetUserById(newUserId);

            Assert.IsNotNull(newUser);
            Assert.AreEqual(newUser.IdsrvUniqueId, newUserStsId);
            Assert.IsNotNull(newUser.NotificationSettings);
            Assert.IsNotNull(newUser.BirthDate);
            Assert.AreEqual(newUser.BirthDate, DateTime.Parse(this.InitialDateString));
        }

        [TestMethod]
        public void We_Can_Update_Initially_Created_User()
        {
            // arrange
            var newUserStsId = "https://boongaloocompanysts/identity3025f46b-3070-4f75-809d-44b7ae5b8e6a";

            var newUserRequest = new NewUserRequestDto()
            {
                About = "About",
                Email = "some@body.com",
                FirstName = "Stecho",
                LastName = "Stefchev",
                Gender = GenderEnum.Male,
                IdsrvUniqueId = newUserStsId,
                PhoneNumber = "+359886585549",
                LanguageIds = new List<Guid>() { Guid.Parse("e9260fb3-5183-4c3e-9bd2-c606d03b7bcb") }
            };

            // act
            var newUserId = this._userService.InsertNewUser(newUserRequest);

            // assert
            var newUser = this._userService.GetUserById(newUserId);

            Assert.IsNotNull(newUser);
            Assert.AreEqual(newUser.IdsrvUniqueId, newUserStsId);
            Assert.IsNotNull(newUser.NotificationSettings);
            Assert.IsNotNull(newUser.BirthDate);
            Assert.AreEqual(newUser.BirthDate, DateTime.Parse(this.InitialDateString));

            // arrange
            var updatedBirthDate = DateTime.UtcNow;
            var updatedAbout = "UpdatedAbout";
            var updatedEmail = "Updatedsome@body.com";
            var firstNameUpdated = "UpdatedStecho";
            var lastNameUpdated = "UpdatedStefchev";
            var firstLanguageId = Guid.Parse("e9260fb3-5183-4c3e-9bd2-c606d03b7bcb");
            var secondLanguageId = Guid.Parse("05872235-365b-41f8-ab50-3913ffe9c601");

            var updateData = new UpdateUserInfoRequestDto()
            {
                About = updatedAbout,
                BirthDate = updatedBirthDate,
                Email = updatedEmail,
                FirstName = firstNameUpdated,
                Gender = GenderEnum.Female,
                LastName = lastNameUpdated,
                LanguageIds = new List<Guid>()
                {
                    firstLanguageId,
                    secondLanguageId
                },
                PhoneNumber = "Updated+359886585549"
            };

            // act
            this._userService.UpdateUserInfo(newUserId, updateData);

            // assert
            newUser = this._userService.GetUserById(newUserId);

            Assert.IsNotNull(newUser);
            Assert.AreEqual(newUser.IdsrvUniqueId, newUserStsId);
            Assert.AreEqual(newUser.BirthDate, updatedBirthDate);
            Assert.AreEqual(newUser.About, updatedAbout);
            Assert.AreEqual(newUser.Email, updatedEmail);
            Assert.AreEqual(newUser.FirstName, firstNameUpdated);
            Assert.AreEqual(newUser.LastName, lastNameUpdated);
            Assert.AreEqual(newUser.Languages.Count(), 2);
            Assert.IsTrue(newUser.Languages.Select(l => l.Id).Contains(firstLanguageId));
            Assert.IsTrue(newUser.Languages.Select(l => l.Id).Contains(secondLanguageId));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "User not available in Database")]
        public void Updating_Non_Existing_User_Throws_ArgumentException()
        {
            // arrange
            var newUserStsId = "https://boongaloocompanysts/identity3025f46b-3070-4f75-809d-44b7ae5b8e6a";
            // TODO: Extract this as a common part between this one and the one above
            var newUserRequest = new NewUserRequestDto()
            {
                About = "About",
                Email = "some@body.com",
                FirstName = "Stecho",
                LastName = "Stefchev",
                Gender = GenderEnum.Male,
                IdsrvUniqueId = newUserStsId,
                PhoneNumber = "+359886585549"
            };

            // act
            var newUserId = this._userService.InsertNewUser(newUserRequest);

            // assert
            var newUser = this._userService.GetUserById(newUserId);

            Assert.IsNotNull(newUser);
            Assert.AreEqual(newUser.IdsrvUniqueId, newUserStsId);
            Assert.IsNotNull(newUser.NotificationSettings);
            Assert.IsNotNull(newUser.BirthDate);
            Assert.AreEqual(newUser.BirthDate, DateTime.Parse(this.InitialDateString));

            var updatedBirthDate = DateTime.UtcNow;
            var updatedAbout = "UpdatedAbout";
            var updatedEmail = "Updatedsome@body.com";
            var firstNameUpdated = "UpdatedStecho";
            var lastNameUpdated = "UpdatedStefchev";
            var firstLanguageId = Guid.Parse("e9260fb3-5183-4c3e-9bd2-c606d03b7bcb");
            var secondLanguageId = Guid.Parse("05872235-365b-41f8-ab50-3913ffe9c601");

            var updateData = new UpdateUserInfoRequestDto()
            {
                About = updatedAbout,
                BirthDate = updatedBirthDate,
                Email = updatedEmail,
                FirstName = firstNameUpdated,
                Gender = GenderEnum.Female,
                LastName = lastNameUpdated,
                LanguageIds = new List<Guid>()
                {
                    firstLanguageId,
                    secondLanguageId
                },
                PhoneNumber = "Updated+359886585549"
            };

            // act

            Guid notValidUserId = Guid.NewGuid();

            while (notValidUserId == newUserId)
                notValidUserId = Guid.NewGuid();


            this._userService.UpdateUserInfo(notValidUserId, updateData);
        }

        [TestMethod]
        public void We_Can_Insert_New_User()
        {
            // arrange
            var firstLanguageId = Guid.Parse("e9260fb3-5183-4c3e-9bd2-c606d03b7bcb");
            var secondLanguageId = Guid.Parse("05872235-365b-41f8-ab50-3913ffe9c601");
            var newUserStsId = "https://boongaloocompanysts/identity3025f46b-3070-4f75-809d-44b7ae5b8e6a";

            var groupOneId = Guid.NewGuid();
            var groupOne = new Group() { Id = groupOneId, Name = "GroupOne", Users = new List<User>() };

            var area = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne }
            };

            groupOne.Areas = new List<Area>() { area };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne },
                Areas = new FakeDbSet<Area>() { area }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));

            var newUserRequestDto = new NewUserRequestDto()
            {
                FirstName = "Stefcho",
                LastName = "Stefchev",
                Email = "stefcho@stefchev.com",
                IdsrvUniqueId = newUserStsId,
                LanguageIds = new List<Guid>() { firstLanguageId, secondLanguageId },
                GroupIds = new List<Guid>() { groupOneId },
                BirthDate = DateTime.Parse(this.InitialDateString)
            };

            // act
            var userId = this._userService.InsertNewUser(newUserRequestDto);

            // assert
            Assert.IsNotNull(userId);

            var user = this._userService.GetUserById(userId);

            Assert.IsNotNull(user);
            Assert.IsNotNull(user.Id);
            Assert.AreEqual(userId, user.Id);
            Assert.AreEqual(user.Languages.Count(), 2);
            Assert.AreEqual(user.BirthDate, DateTime.Parse(this.InitialDateString));
        }

        [TestMethod]
        public void We_Can_Get_Notification_Settings_For_User()
        {
            // arrange
            var newUserStsId = "https://boongaloocompanysts/identity3025f46b-3070-4f75-809d-44b7ae5b8e6a";

            var newUserRequest = new NewUserRequestDto()
            {
                About = "About",
                Email = "some@body.com",
                FirstName = "Stecho",
                Gender = GenderEnum.Male,
                IdsrvUniqueId = newUserStsId,
                PhoneNumber = "+359886585549"
            };

            // act
            var newUserId = this._userService.InsertNewUser(newUserRequest);

            // assert
            var newUser = this._userService.GetUserById(newUserId);

            Assert.IsNotNull(newUser);

            // arrange
            // 1. Create new update user notifications request with tags 
            var newUpdatedUserNotifications = new EditUserAutomaticSubscriptionSettingsRequestDto()
            {
                AutomaticallySubscribeToAllGroups = false,
                AutomaticallySubscribeToAllGroupsWithTag = true,
                SubscribedTagIds = new List<Guid>()
                {
                    Guid.Parse("caf77dee-a94f-49cb-b51f-e0c0e1067541"), Guid.Parse("bd456f08-f137-4382-8358-d52772c2dfc8")
                }
            };

            // act
            this._userService.UpdateUserNotificationSettings(newUserId, newUpdatedUserNotifications);

            // assert
            var updatedNotificationSettings = this._userService.GetNotificationSettingsForUserWithId(newUserId);

            Assert.IsNotNull(updatedNotificationSettings);
            Assert.IsNotNull(updatedNotificationSettings.SubscribedTagIds);
            // 1. Notificiation settings were updated and now we have tags
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "There's no notification settings for the user")]
        public void Exception_Is_Thrown_When_Trying_To_Update_Not_Existing_Notification()
        {
            // arrange
            var userId = Guid.NewGuid();
            var dbUser = new User()
            {
                Id = userId,
                NotificationSettings = null
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Users = new FakeDbSet<User>() { dbUser }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));

            // act
            this._userService.UpdateUserNotificationSettings(userId, null);
        }

        [TestMethod]
        public void We_Can_Get_All_Users()
        {
            // arrange
            var firstUserId = Guid.NewGuid();
            var firstUser = new User()
            {
                Id = firstUserId,
                FirstName = "Stefcho",
                LastName = "Stefchev",
                Email = "mail@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString()
            };

            var secondUserId = Guid.NewGuid();
            var secondUser = new User()
            {
                Id = secondUserId,
                FirstName = "Other",
                LastName = "Name",
                Email = "null@null.bg",
                IdsrvUniqueId = Guid.NewGuid().ToString()
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Users = new FakeDbSet<User>() { firstUser, secondUser }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));

            // act
            var users = this._userService.GetAllUsers();

            // assert
            Assert.IsNotNull(users);
            Assert.IsTrue(users.Count() == 2);
        }

        [TestMethod]
        public void We_Can_Get_User_By_StsId()
        {
            // arrange
            var idsrvUniqueId = Guid.NewGuid().ToString();
            var firstUser = new User()
            {
                Id = Guid.NewGuid(),
                FirstName = "Stefcho",
                LastName = "Stefchev",
                Email = "mail@mail.ma",
                IdsrvUniqueId = idsrvUniqueId
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Users = new FakeDbSet<User>() { firstUser }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));

            // act
            var user = this._userService.GetUserByStsId(idsrvUniqueId);

            // assert
            Assert.IsNotNull(user);
        }

        [TestMethod]
        public void When_User_Location_Is_Updated_Proper_ResponseDto_IsReturned()
        {
            // arrange
            var userId = Guid.NewGuid();
            var user = new User()
            {
                Id = userId,
                FirstName = "Stefcho",
                LastName = "Stefchev",
                Email = "mail@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString(),
                NotificationSettings = new NotificationSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                }
            };

            var groupOneId = Guid.NewGuid();
            var groupTwoId = Guid.NewGuid();

            var groupOne = new Group() { Id = groupOneId, Name = "GroupOne", IsActive = true, Users = new List<User>() };
            var groupTwo = new Group() { Id = groupTwoId, Name = "GroupTwo", IsActive = true, Users = new List<User>() };

            var areaId = Guid.NewGuid();
            var area = new Area()
            {
                Id = areaId,
                Latitude = 10.00000,
                Longitude = 10.00000,
                Groups = new List<Group>() { groupOne, groupTwo },
                Radius = RadiusRangeEnum.FiftyMeters,
                IsActive = true
            };

            groupOne.Areas = new List<Area>() { area };
            groupTwo.Areas = new List<Area>() { area };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo },
                Areas = new FakeDbSet<Area>() { area },
                Users = new FakeDbSet<User>() { user }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));

            // act
            var relateUserToGroupsDto = new RelateUserToGroupsDto()
            {
                UserId = userId,
                Latitude = 10,
                Longitude = 10,
                GroupsUserSubscribes = new List<Guid>() { groupOneId, groupTwoId }
            };

            this._subscriptionService.RelateUserToGroups(relateUserToGroupsDto);
            Assert.IsNotNull(user.Groups);
            Assert.IsNotNull(user.HistoryGroups);

            Assert.AreEqual(2, user.Groups.Count);
            Assert.AreEqual(2, user.HistoryGroups.Count);

            this._userService.UpdateUserLocation(userId, 20, 20);

            Assert.AreEqual(0, user.Groups.Count);
            Assert.AreEqual(2, user.HistoryGroups.Count);

            var updateResponse = this._userService.UpdateUserLocation(userId, 10.00009, 10.00009);

            // TODO: When implemented double SecondsToClosestBoundary(double latitude, double longitude); update the assertions here
            Assert.AreEqual(2, updateResponse.SubscribedGroupIds.Count());
        }

        [TestMethod]
        public void HistoryGroups_And_RegularGroups_Are_Correct_When_Updating_Location_With_SubscribeToAllOption_TurnedOn()
        {
            // arrange
            var userId = Guid.NewGuid();
            var user = new User()
            {
                Id = userId,
                FirstName = "Stefcho",
                LastName = "Stefchev",
                Email = "mail@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString(),
                NotificationSettings = new NotificationSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                }
            };

            // ================= Area1 (GR1, GR3) ========
            var groupOneId = Guid.NewGuid();
            var groupOne = new Group()
            {
                Id = groupOneId,
                Name = "Group one",
                IsActive = true
            };

            var groupThreeId = Guid.NewGuid();
            var groupThree = new Group()
            {
                Id = groupThreeId,
                Name = "Group three",
                IsActive = true
            };

            var areaOneId = Guid.NewGuid();
            var areaOne = new Area()
            {
                Id = areaOneId,
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne, groupThree },
                Radius = RadiusRangeEnum.FiftyMeters,
                IsActive = true
            };
            // ================= Area1 (GR1, GR3) ========

            // ================= Area2 (GR3) ========
            var groupSixId = Guid.NewGuid();
            var groupSix = new Group()
            {
                Id = groupSixId,
                Name = "Group six",
                IsActive = true
            };

            var areaTwoId = Guid.NewGuid();
            var areaTwo = new Area()
            {
                Id = areaTwoId,
                Latitude = 20,
                Longitude = 20,
                Groups = new List<Group>() { groupSix },
                Radius = RadiusRangeEnum.FiftyMeters,
                IsActive = true
            };
            // ================= Area2 (GR3) ========

            groupOne.Areas = new List<Area>() { areaOne };
            groupThree.Areas = new List<Area>() { areaOne };

            groupSix.Areas = new List<Area>() { areaTwo };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne, groupThree, groupSix },
                Areas = new FakeDbSet<Area>() { areaOne, areaTwo },
                Users = new FakeDbSet<User>() { user }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));

            // act
            var groupsDependingOnUserSettingsAroundCoordinates = this._userService.UpdateUserLocation(userId, 10, 10);

            // assert
            Assert.AreEqual(2, groupsDependingOnUserSettingsAroundCoordinates.SubscribedGroupIds.Count());
            Assert.IsTrue(groupsDependingOnUserSettingsAroundCoordinates.SubscribedGroupIds.Contains(groupOneId) &&
                            groupsDependingOnUserSettingsAroundCoordinates.SubscribedGroupIds.Contains(groupThreeId));
            Assert.IsTrue(user.Groups.Contains(groupOne) && user.Groups.Contains(groupThree));
            Assert.IsTrue(user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupOne.Id) &&
                            user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupThree.Id));

            // act
            groupsDependingOnUserSettingsAroundCoordinates = this._userService.UpdateUserLocation(userId, 15, 15);

            // assert
            Assert.AreEqual(0, groupsDependingOnUserSettingsAroundCoordinates.SubscribedGroupIds.Count());
            Assert.IsFalse(user.Groups.Contains(groupOne) && user.Groups.Contains(groupThree));
            Assert.IsTrue(user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupOne.Id) &&
                            user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupThree.Id));

            // act
            groupsDependingOnUserSettingsAroundCoordinates = this._userService.UpdateUserLocation(userId, 20, 20);

            // assert
            Assert.AreEqual(1, groupsDependingOnUserSettingsAroundCoordinates.SubscribedGroupIds.Count());
            Assert.IsTrue(groupsDependingOnUserSettingsAroundCoordinates.SubscribedGroupIds.Contains(groupSixId));
            Assert.IsTrue(user.Groups.Contains(groupSix));
            Assert.IsTrue(user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupOne.Id) &&
                            user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupThree.Id) &&
                            user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupSix.Id));

            // act
            groupsDependingOnUserSettingsAroundCoordinates = this._userService.UpdateUserLocation(userId, 25, 25);

            // assert
            Assert.AreEqual(0, groupsDependingOnUserSettingsAroundCoordinates.SubscribedGroupIds.Count());
            Assert.IsFalse(user.Groups.Any());
            Assert.IsTrue(user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupOne.Id) &&
                            user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupThree.Id) &&
                            user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupSix.Id));

            // act
            groupsDependingOnUserSettingsAroundCoordinates = this._userService.UpdateUserLocation(userId, 10, 10);

            // assert
            Assert.AreEqual(2, groupsDependingOnUserSettingsAroundCoordinates.SubscribedGroupIds.Count());
            Assert.IsTrue(groupsDependingOnUserSettingsAroundCoordinates.SubscribedGroupIds.Contains(groupOneId) &&
                           groupsDependingOnUserSettingsAroundCoordinates.SubscribedGroupIds.Contains(groupThreeId));
            Assert.IsTrue(user.Groups.Contains(groupOne) && user.Groups.Contains(groupThree));
            Assert.IsTrue(user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupOne.Id) &&
                            user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupThree.Id) &&
                            user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupSix.Id));
        }

        [TestMethod]
        public void HistoryGroups_And_RegularGroups_Are_Correct_When_Updating_Location_With_SubscribeToAllWithTagOption_TurnedOn()
        {
            throw new NotImplementedException();
        }
    }
}
