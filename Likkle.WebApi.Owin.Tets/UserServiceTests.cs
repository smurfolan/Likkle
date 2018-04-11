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
        private readonly Mock<ISignalrService> _signalrServiceMock;
        private readonly Mock<ISubscriptionService> _subscripionServiceMock;
        private readonly Mock<IAreaService> _areaServiceMock;

        public UserServiceTests()
        {
            var fakeDbContext = new FakeLikkleDbContext().Seed();
            _mockedLikkleUoW = new Mock<ILikkleUoW>();
            _mockedLikkleUoW.Setup(uow => uow.AreaRepository)
                .Returns(new AreaRepository(fakeDbContext));
            _mockedLikkleUoW.Setup(uow => uow.UserRepository)
                .Returns(new UserRepository(fakeDbContext));
            _mockedLikkleUoW.Setup(uow => uow.GroupRepository)
                .Returns(new GroupRepository(fakeDbContext));
            _mockedLikkleUoW.Setup(uow => uow.TagRepository)
                .Returns(new TagRepository(fakeDbContext));
            _mockedLikkleUoW.Setup(uow => uow.LanguageRepository)
                .Returns(new LanguageRepository(fakeDbContext));

            _accelometerAlgorithmHelperService = new Mock<IAccelometerAlgorithmHelperService>();
            _accelometerAlgorithmHelperService.Setup(
                a => a.SecondsToClosestBoundary(It.IsAny<double>(), It.IsAny<double>())).Returns(32.5);

            _subscriptionSettingsServiceMock = new Mock<ISubscriptionSettingsService>();
            _subscripionServiceMock = new Mock<ISubscriptionService>();

            _signalrServiceMock = new Mock<ISignalrService>();


            _mockedConfigurationProvider = new Mock<IConfigurationProvider>();

            var mapConfiguration = new MapperConfiguration(cfg => {
                cfg.AddProfile<EntitiesMappingProfile>();
            });
            _mockedConfigurationProvider.Setup(mc => mc.CreateMapper()).Returns(mapConfiguration.CreateMapper);

            _subscriptionSettingsService = new SubscriptionSettingsService(
                _mockedLikkleUoW.Object,
                _mockedConfigurationProvider.Object);

            _configurationWrapperMock = new Mock<IConfigurationWrapper>();

            // Only this call to the mocked service returns the actual result but not mocked one.
            _subscriptionSettingsServiceMock
                .Setup(ss => ss.GroupsForUserAroundCoordinatesBasedOnUserSettings(It.IsAny<Guid>(), It.IsAny<double>(), It.IsAny<double>()))
                .Returns((Guid userId, double lat, double lon) => _subscriptionSettingsService.GroupsForUserAroundCoordinatesBasedOnUserSettings(userId, lat, lon));
                
            _userService = new UserService(
                _mockedLikkleUoW.Object,
                _mockedConfigurationProvider.Object,
                _configurationWrapperMock.Object,
                _accelometerAlgorithmHelperService.Object,
                _subscriptionSettingsServiceMock.Object,
                _subscripionServiceMock.Object);

            _subscriptionService = new SubscriptionService(
                _mockedLikkleUoW.Object, 
                _mockedConfigurationProvider.Object, 
                _configurationWrapperMock.Object,
                _signalrServiceMock.Object);
        }

        [TestMethod]
        public void InsertNewUser_We_Can_Create_New_User_Without_Groups_And_Languages()
        {
            // arrange
            var newUserStsId = DataGenerator.GetUniequeStsID();

            var newUserRequest = new NewUserRequestDto()
            {
                IdsrvUniqueId = newUserStsId
            };

            // act
            var newUserId = _userService.InsertNewUser(newUserRequest);

            // assert
            var newUser = _userService.GetUserById(newUserId);

            Assert.IsNotNull(newUser);
            Assert.AreEqual(newUser.IdsrvUniqueId, newUserStsId);
            Assert.IsNotNull(newUser.AutomaticSubscriptionSettings);
            Assert.IsNotNull(newUser.BirthDate);
            Assert.AreEqual(newUser.BirthDate, DateTime.Parse(InitialDateString));
        }

        [TestMethod]
        public void UpdateUserInfo_We_Can_Update_Initially_Created_User()
        {
            // arrange
            var newUserStsId = DataGenerator.GetUniequeStsID();

            var newUserRequest = new NewUserRequestDto()
            {
                IdsrvUniqueId = newUserStsId
            };

            // act
            var newUserId = this._userService.InsertNewUser(newUserRequest);

            // assert
            var newUser = this._userService.GetUserById(newUserId);

            Assert.IsNotNull(newUser);
            Assert.AreEqual(newUser.IdsrvUniqueId, newUserStsId);
            Assert.IsNotNull(newUser.AutomaticSubscriptionSettings);
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
        public void UpdateUserInfo_Updating_Non_Existing_User_Throws_ArgumentException()
        {
            // arrange
            var newUserStsId = DataGenerator.GetUniequeStsID();

            var newUserRequest = new NewUserRequestDto()
            {
                IdsrvUniqueId = newUserStsId
            };

            // act
            var newUserId = this._userService.InsertNewUser(newUserRequest);

            // assert
            var newUser = this._userService.GetUserById(newUserId);

            Assert.IsNotNull(newUser);
            Assert.AreEqual(newUser.IdsrvUniqueId, newUserStsId);
            Assert.IsNotNull(newUser.AutomaticSubscriptionSettings);
            Assert.IsNotNull(newUser.BirthDate);
            Assert.AreEqual(newUser.BirthDate, DateTime.Parse(this.InitialDateString));

            var updateData = new UpdateUserInfoRequestDto(){};
            // act
            Guid notValidUserId = Guid.NewGuid();

            while (notValidUserId == newUserId)
                notValidUserId = Guid.NewGuid();

            this._userService.UpdateUserInfo(notValidUserId, updateData);
        }

        [TestMethod]
        public void InsertNewUser_We_Can_Insert_New_User()
        {
            // arrange
            var firstLanguageId = FakeLikkleDbContext.GetAllAvailableLanguages().ToArray()[0].Key;
            var secondLanguageId = FakeLikkleDbContext.GetAllAvailableLanguages().ToArray()[1].Key;
            var newUserStsId = DataGenerator.GetUniequeStsID();

            var groupOneId = Guid.NewGuid();
            var groupOne = new Group() { Id = groupOneId, Users = new List<User>() };

            var newUserRequestDto = new NewUserRequestDto()
            {
                LanguageIds = new List<Guid>() { firstLanguageId, secondLanguageId }
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
        public void GetAutomaticSubscriptionSettingsForUserWithId_We_Can_Get_Automatic_Subscription_Settings_For_User()
        {
            // arrange
            var newUserStsId = DataGenerator.GetUniequeStsID();

            var newUserRequest = new NewUserRequestDto() { };

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
                    FakeLikkleDbContext.GetAllAvailableTags().ToArray()[0].Key,
                    FakeLikkleDbContext.GetAllAvailableTags().ToArray()[1].Key
                }
            };

            // act
            this._userService.UpdateUserAutomaticSubscriptionSettings(newUserId, newUpdatedUserNotifications);

            // assert
            var updatedSubscriptionSettings = this._userService.GetAutomaticSubscriptionSettingsForUserWithId(newUserId);

            Assert.IsNotNull(updatedSubscriptionSettings);
            Assert.IsNotNull(updatedSubscriptionSettings.SubscribedTagIds);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "There's no notification settings for the user")]
        public void UpdateUserAutomaticSubscriptionSettings_Exception_Is_Thrown_When_Trying_To_Update_Not_Existing_Notification()
        {
            // arrange
            var userId = Guid.NewGuid();
            var dbUser = new User()
            {
                Id = userId,
                AutomaticSubscriptionSettings = null
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Users = new FakeDbSet<User>() { dbUser }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));

            // act
            this._userService.UpdateUserAutomaticSubscriptionSettings(userId, null);
        }

        [TestMethod]
        public void GetAllUsers_We_Can_Get_All_Users()
        {
            // arrange
            var firstUser = new User() { };
            var secondUser = new User() { };

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
        public void GetUserByStsId_We_Can_Get_User_By_StsId()
        {
            // arrange
            var idsrvUniqueId = Guid.NewGuid().ToString();
            var firstUser = new User()
            {
                IdsrvUniqueId = idsrvUniqueId,
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = false,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                }
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
        public void RelateUserToGroups_When_User_Location_Is_Updated_Proper_ResponseDto_IsReturned()
        {
            // arrange
            var userId = Guid.NewGuid();
            var user = new User()
            {
                Id = userId,
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Groups = new List<Group>()
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

            // assert
            Assert.IsNotNull(user.Groups);
            Assert.IsNotNull(user.HistoryGroups);

            Assert.AreEqual(2, user.Groups.Count);
            Assert.AreEqual(2, user.HistoryGroups.Count);

            this._userService.UpdateUserLocation(userId, 20, 20);
            
            Assert.AreEqual(0, user.Groups.Count);
            Assert.AreEqual(2, user.HistoryGroups.Count);

            this._signalrServiceMock.Verify(srm => srm.GroupWasJoinedByUser(It.IsAny<Guid>(), It.IsAny<List<string>>()), Times.Never);

            var updateResponse = this._userService.UpdateUserLocation(userId, 10.00009, 10.00009);
            
            Assert.AreEqual(2, updateResponse.SubscribedGroupIds.Count());
            Assert.AreEqual(32.5, updateResponse.SecodsToClosestBoundary);
        }

        [TestMethod]
        public void UpdateUserLocation_HistoryGroups_And_RegularGroups_Are_Correct_When_Updating_Location_With_SubscribeToAllOption_TurnedOn()
        {
            // arrange
            var userId = Guid.NewGuid();
            var user = new User()
            {
                Id = userId,
                IdsrvUniqueId = Guid.NewGuid().ToString(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
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
                IsActive = true
            };

            var groupThreeId = Guid.NewGuid();
            var groupThree = new Group()
            {
                Id = groupThreeId,
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

            this._subscripionServiceMock.Verify(ssm => ssm.AutoDecreaseUsersInGroups(It.IsAny<IEnumerable<Guid>>(), It.IsAny<Guid>()), Times.Exactly(2));

            // act
            groupsDependingOnUserSettingsAroundCoordinates = this._userService.UpdateUserLocation(userId, 15, 15);

            // assert
            Assert.AreEqual(0, groupsDependingOnUserSettingsAroundCoordinates.SubscribedGroupIds.Count());
            Assert.IsFalse(user.Groups.Contains(groupOne) && user.Groups.Contains(groupThree));
            Assert.IsTrue(user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupOne.Id) &&
                            user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupThree.Id));

            this._subscripionServiceMock.Verify(ssm => ssm.AutoDecreaseUsersInGroups(It.IsAny<IEnumerable<Guid>>(), It.IsAny<Guid>()), Times.Exactly(4));
            // act
            groupsDependingOnUserSettingsAroundCoordinates = this._userService.UpdateUserLocation(userId, 20, 20);

            // assert
            Assert.AreEqual(1, groupsDependingOnUserSettingsAroundCoordinates.SubscribedGroupIds.Count());
            Assert.IsTrue(groupsDependingOnUserSettingsAroundCoordinates.SubscribedGroupIds.Contains(groupSixId));
            Assert.IsTrue(user.Groups.Contains(groupSix));
            Assert.IsTrue(user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupOne.Id) &&
                            user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupThree.Id) &&
                            user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupSix.Id));

            this._subscripionServiceMock.Verify(ssm => ssm.AutoDecreaseUsersInGroups(It.IsAny<IEnumerable<Guid>>(), It.IsAny<Guid>()), Times.Exactly(5));

            // act
            groupsDependingOnUserSettingsAroundCoordinates = this._userService.UpdateUserLocation(userId, 25, 25);

            // assert
            Assert.AreEqual(0, groupsDependingOnUserSettingsAroundCoordinates.SubscribedGroupIds.Count());
            Assert.IsFalse(user.Groups.Any());
            Assert.IsTrue(user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupOne.Id) &&
                            user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupThree.Id) &&
                            user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupSix.Id));

            this._subscripionServiceMock.Verify(ssm => ssm.AutoDecreaseUsersInGroups(It.IsAny<IEnumerable<Guid>>(), It.IsAny<Guid>()), Times.Exactly(6));
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

            this._subscripionServiceMock.Verify(ssm => ssm.AutoIncreaseUsersInGroups(It.IsAny<IEnumerable<Guid>>(), It.IsAny<Guid>()), Times.Exactly(2));
        }

        [TestMethod]
        public void UpdateUserLocation_HistoryGroups_And_RegularGroups_Are_Correct_When_Updating_Location_With_SubscribeToAllWithTagOption_TurnedOn()
        {
            var allTags = this._mockedLikkleUoW.Object.TagRepository.GetAllTags().ToList();

            // arrange
            var user = new User()
            {
                Id = Guid.NewGuid(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = false,
                    AutomaticallySubscribeToAllGroupsWithTag = true,
                    Tags = allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList()
                }
            };

            // ================= Area1 (GR1, GR2) ========
            var groupOne = new Group()
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                Tags = new List<Tag>()
                {
                    allTags.FirstOrDefault(t => t.Name == "Animals")
                }
            };
            
            var groupTwo = new Group()
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                Tags = new List<Tag>()
                {
                    allTags.FirstOrDefault(t => t.Name == "Sport")
                }
            };
            
            var areaOne = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne, groupTwo },
                IsActive = true,
                Radius = RadiusRangeEnum.FiftyMeters
            };

            groupOne.Areas = new List<Area>() { areaOne };
            groupTwo.Areas = new List<Area>() { areaOne };

            // ================= Area1 (GR1, GR2) ========
            // ================= Area1 (GR3, GR4, GR5) ========
            var groupThree = new Group()
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                Tags = new List<Tag>()
                {
                    allTags.FirstOrDefault(t => t.Name == "Help"),
                    allTags.FirstOrDefault(t => t.Name == "Sport")
                }
            };

            var groupFour = new Group()
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                Tags = new List<Tag>()
                {
                    allTags.FirstOrDefault(t => t.Name == "University")
                }
            };
            
            var groupFive = new Group()
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                Tags = new List<Tag>()
                {
                    allTags.FirstOrDefault(t => t.Name == "Help"),
                    allTags.FirstOrDefault(t => t.Name == "Animals")
                }
            };
            
            var areaTwo = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = 20,
                Longitude = 20,
                Groups = new List<Group>() { groupThree, groupFour, groupFive },
                IsActive = true,
                Radius = RadiusRangeEnum.FiftyMeters
            };

            groupThree.Areas = new List<Area>() { areaTwo };
            groupFour.Areas = new List<Area>() { areaTwo };
            groupFive.Areas = new List<Area>() { areaTwo };
            // ================= Area1 (GR3, GR4, GR5) ========

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo, groupThree, groupFour, groupFive },
                Areas = new FakeDbSet<Area>() { areaOne, areaTwo },
                Users = new FakeDbSet<User>() { user }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));

            // act
            var groupsDependingOnUserSettingsAroundCoordinates = this._userService.UpdateUserLocation(user.Id, 10, 10);

            // assert
            Assert.AreEqual(1, groupsDependingOnUserSettingsAroundCoordinates.SubscribedGroupIds.Count());
            Assert.IsTrue(groupsDependingOnUserSettingsAroundCoordinates.SubscribedGroupIds.Contains(groupTwo.Id));

            Assert.IsTrue(user.Groups.Contains(groupTwo));
            Assert.AreEqual(1, user.Groups.Count());

            Assert.IsTrue(user.HistoryGroups.Select(gr => gr.GroupId).Contains(groupTwo.Id));
            Assert.AreEqual(1, user.HistoryGroups.Count());

            this._subscripionServiceMock.Verify(ssm => ssm.AutoDecreaseUsersInGroups(It.IsAny<IEnumerable<Guid>>(), It.IsAny<Guid>()), Times.Once);

            // act
            groupsDependingOnUserSettingsAroundCoordinates = this._userService.UpdateUserLocation(user.Id, 15, 15);

            // assert 
            Assert.IsFalse(groupsDependingOnUserSettingsAroundCoordinates.SubscribedGroupIds.Any());
            Assert.IsFalse(user.Groups.Any());
            Assert.AreEqual(1, user.HistoryGroups.Count());
            Assert.IsTrue(user.HistoryGroups.Select(hgr => hgr.GroupId).Contains(groupTwo.Id));

            this._subscripionServiceMock.Verify(ssm => ssm.AutoDecreaseUsersInGroups(It.IsAny<IEnumerable<Guid>>(), It.IsAny<Guid>()), Times.Exactly(2));

            // act
            groupsDependingOnUserSettingsAroundCoordinates = this._userService.UpdateUserLocation(user.Id, 20, 20);

            // assert
            Assert.AreEqual(2, groupsDependingOnUserSettingsAroundCoordinates.SubscribedGroupIds.Count());
            Assert.AreEqual(2, user.Groups.Count());
            Assert.IsTrue(user.Groups.Contains(groupThree) && user.Groups.Contains(groupFive));

            Assert.AreEqual(3, user.HistoryGroups.Count());
            Assert.IsTrue(user.HistoryGroups.Select(hgr => hgr.GroupThatWasPreviouslySubscribed).Contains(groupTwo) &&
                            user.HistoryGroups.Select(hgr => hgr.GroupThatWasPreviouslySubscribed).Contains(groupThree) &&
                            user.HistoryGroups.Select(hgr => hgr.GroupThatWasPreviouslySubscribed).Contains(groupFive));

            this._subscripionServiceMock.Verify(ssm => ssm.AutoDecreaseUsersInGroups(It.IsAny<IEnumerable<Guid>>(), It.IsAny<Guid>()), Times.Exactly(4));

            // act
            groupsDependingOnUserSettingsAroundCoordinates = this._userService.UpdateUserLocation(user.Id, 25, 25);

            // assert
            Assert.IsFalse(groupsDependingOnUserSettingsAroundCoordinates.SubscribedGroupIds.Any());

            Assert.AreEqual(0, user.Groups.Count());

            Assert.AreEqual(3, user.HistoryGroups.Count());
            Assert.IsTrue(user.HistoryGroups.Select(hgr => hgr.GroupThatWasPreviouslySubscribed).Contains(groupTwo) &&
                            user.HistoryGroups.Select(hgr => hgr.GroupThatWasPreviouslySubscribed).Contains(groupThree) &&
                            user.HistoryGroups.Select(hgr => hgr.GroupThatWasPreviouslySubscribed).Contains(groupFive));

            this._subscripionServiceMock.Verify(ssm => ssm.AutoDecreaseUsersInGroups(It.IsAny<IEnumerable<Guid>>(), It.IsAny<Guid>()), Times.Exactly(6));

            // act
            groupsDependingOnUserSettingsAroundCoordinates = this._userService.UpdateUserLocation(user.Id, 10, 10);

            // assert
            Assert.AreEqual(1, groupsDependingOnUserSettingsAroundCoordinates.SubscribedGroupIds.Count());

            Assert.AreEqual(1, user.Groups.Count());
            Assert.IsTrue(user.Groups.Contains(groupTwo));

            var histGroups = user.HistoryGroups.Select(hgr => hgr.GroupThatWasPreviouslySubscribed);

            this._subscripionServiceMock.Verify(ssm => ssm.AutoIncreaseUsersInGroups(It.IsAny<IEnumerable<Guid>>(), It.IsAny<Guid>()), Times.Exactly(1));

            Assert.AreEqual(3, user.HistoryGroups.Count());
            Assert.IsTrue(histGroups.Contains(groupTwo) && histGroups.Contains(groupThree) && histGroups.Contains(groupFive));
        }

        [TestMethod]
        public void Disable_When_User_Disabled_All_His_Groups_Are_Not_Assiciated_With_Him_Anymore()
        {
            // arrange
            var allTags = this._mockedLikkleUoW.Object.TagRepository.GetAllTags().ToList();

            var user = new User() { Id = Guid.NewGuid() };

            var groupOne = new Group()
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                Tags = new List<Tag>()
                {
                    allTags.FirstOrDefault(t => t.Name == "Animals")
                },
                Users = new List<User>() { user }
            };

            var groupTwo = new Group()
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                Tags = new List<Tag>()
                {
                    allTags.FirstOrDefault(t => t.Name == "Sport")
                },
                Users = new List<User>() { user }
            };

            var areaOneId = Guid.NewGuid();
            var areaOne = new Area()
            {
                Id = areaOneId,
                Groups = new List<Group>() { groupOne, groupTwo },
                IsActive = true,
                Radius = RadiusRangeEnum.FiftyMeters
            };

            groupOne.Areas = new List<Area>() { areaOne };
            groupTwo.Areas = new List<Area>() { areaOne };

            user.Groups = new List<Group>() { groupOne, groupTwo };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo },
                Areas = new FakeDbSet<Area>() { areaOne },
                Users = new FakeDbSet<User>() { user }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));

            Assert.AreEqual(2, user.Groups.Count);
            Assert.IsTrue(user.Groups.Contains(groupOne));
            Assert.IsTrue(user.Groups.Contains(groupTwo));

            // act
            this._userService.Disable(user.Id);

            // assert
            Assert.AreEqual(0, user.Groups.Count);
            this._subscripionServiceMock.Verify(ssm => ssm.AutoDecreaseUsersInGroups(It.IsAny<IEnumerable<Guid>>(), It.IsAny<Guid>()), Times.Exactly(2));
        }
    }
}
