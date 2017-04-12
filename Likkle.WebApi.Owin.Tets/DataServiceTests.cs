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
    // TODO: Remove repeating code.There's plenty of it.
    // TODO: Implement random data generator and apply it here
    // TODO: Try to remove all hard coded values
    [TestClass]
    public class DataServiceTests
    {
        private readonly string InitialDateString = "01/01/1753";

        private readonly DataService _dataService;
        private readonly Mock<ILikkleUoW> _mockedLikkleUoW;
        private readonly Mock<IConfigurationProvider> _mockedConfigurationProvider;
        private readonly Mock<IConfigurationWrapper> _configurationWrapperMock;

        public DataServiceTests()
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

            this._mockedConfigurationProvider = new Mock<IConfigurationProvider>();

            var mapConfiguration = new MapperConfiguration(cfg => {
                cfg.AddProfile<EntitiesMappingProfile>();
            });
            this._mockedConfigurationProvider.Setup(mc => mc.CreateMapper()).Returns(mapConfiguration.CreateMapper);

            this._configurationWrapperMock = new Mock<IConfigurationWrapper>();

            this._dataService = new DataService(
                this._mockedLikkleUoW.Object, 
                this._mockedConfigurationProvider.Object,
                this._configurationWrapperMock.Object);
        }

        [TestMethod]
        public void We_Can_Relate_User_To_Groups()
        {
            // arrange
            var groupOneId = Guid.NewGuid();
            var groupTwoId = Guid.NewGuid();

            var groupOne = new Group() { Id = groupOneId, Name = "GroupOne", Users = new List<User>()};
            var groupTwo = new Group() { Id = groupTwoId, Name = "GroupTwo", Users = new List<User>()};

            var area = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne, groupTwo }
            };

            groupOne.Areas = new List<Area>() { area };
            groupTwo.Areas = new List<Area>() { area };

            var userId = Guid.NewGuid();
            var user = new User()
            {
                Id = userId,
                FirstName = "Stefcho",
                LastName = "Stefchev",
                Email = "mail@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString()
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>(){ groupOne, groupTwo },
                Areas = new FakeDbSet<Area>() { area },
                Users = new FakeDbSet<User>() { user }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));

            var relateUserToGroupsRequest = new RelateUserToGroupsDto()
            {
                UserId = userId,
                Latitude = 10,
                Longitude = 10,
                GroupsUserSubscribes = new List<Guid>() { groupOneId, groupTwoId }
            };

            // act
            this._dataService.RelateUserToGroups(relateUserToGroupsRequest);
            var userSubscribtionsAroundCoordintes = this._dataService
                .GetUserSubscriptions(userId, 10, 10);

            // assert
            Assert.IsNotNull(userSubscribtionsAroundCoordintes);
            Assert.AreEqual(userSubscribtionsAroundCoordintes.Count(), 2);
            Assert.IsTrue(userSubscribtionsAroundCoordintes.Contains(groupOneId));
            Assert.IsTrue(userSubscribtionsAroundCoordintes.Contains(groupTwoId));

            // arrange
            relateUserToGroupsRequest = new RelateUserToGroupsDto()
            {
                UserId = userId,
                Latitude = 10,
                Longitude = 10,
                GroupsUserSubscribes = new List<Guid>() { groupTwoId }
            };

            groupOne.Users = new List<User>() { user };
            groupTwo.Users = new List<User>() { user };

            // act
            this._dataService.RelateUserToGroups(relateUserToGroupsRequest);
            userSubscribtionsAroundCoordintes = this._dataService.GetUserSubscriptions(userId, 10, 10);

            // assert
            Assert.IsNotNull(userSubscribtionsAroundCoordintes);
            Assert.AreEqual(userSubscribtionsAroundCoordintes.Count(), 1);
            Assert.IsFalse(userSubscribtionsAroundCoordintes.Contains(groupOneId));
            Assert.IsTrue(userSubscribtionsAroundCoordintes.Contains(groupTwoId));

            // arrange
            var groupThree = new Group() { Id = Guid.NewGuid(), Name = "GroupThree", Users = new List<User>() };
            var groupFour = new Group() { Id = Guid.NewGuid(), Name = "GroupFour", Users = new List<User>() };

            groupThree.Areas = new List<Area>() { area };
            groupFour.Areas = new List<Area>() { area };

            area.Groups.Clear();

            area.Groups.Add(groupThree);
            area.Groups.Add(groupFour);

            populatedDatabase.Groups = new FakeDbSet<Group>() { groupThree, groupFour };

            relateUserToGroupsRequest = new RelateUserToGroupsDto()
            {
                UserId = userId,
                Latitude = 10,
                Longitude = 10,
                GroupsUserSubscribes = new List<Guid>() { groupThree.Id, groupFour.Id }
            };

            // act
            this._dataService.RelateUserToGroups(relateUserToGroupsRequest);
            userSubscribtionsAroundCoordintes = this._dataService.GetUserSubscriptions(userId, 10, 10);

            // assert
            Assert.IsNotNull(userSubscribtionsAroundCoordintes);
            Assert.AreEqual(userSubscribtionsAroundCoordintes.Count(), 2);
            Assert.IsFalse(userSubscribtionsAroundCoordintes.Contains(groupOneId));
            Assert.IsFalse(userSubscribtionsAroundCoordintes.Contains(groupTwoId));
            Assert.IsTrue(userSubscribtionsAroundCoordintes.Contains(groupThree.Id));
            Assert.IsTrue(userSubscribtionsAroundCoordintes.Contains(groupFour.Id));         
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
            var newUserId = this._dataService.InsertNewUser(newUserRequest);

            // assert
            var newUser = this._dataService.GetUserById(newUserId);

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
            var newUserId = this._dataService.InsertNewUser(newUserRequest);

            // assert
            var newUser = this._dataService.GetUserById(newUserId);

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
            this._dataService.UpdateUserInfo(newUserId, updateData);

            // assert
            newUser = this._dataService.GetUserById(newUserId);

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
            var newUserId = this._dataService.InsertNewUser(newUserRequest);

            // assert
            var newUser = this._dataService.GetUserById(newUserId);

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


            this._dataService.UpdateUserInfo(notValidUserId, updateData);
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
                BirthDate = DateTime.Parse(this.InitialDateString).AddYears(-1)
            };

            // act
            var userId = this._dataService.InsertNewUser(newUserRequestDto);

            // assert
            Assert.IsNotNull(userId);

            var user = this._dataService.GetUserById(userId);

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
            var newUserId = this._dataService.InsertNewUser(newUserRequest);

            // assert
            var newUser = this._dataService.GetUserById(newUserId);

            Assert.IsNotNull(newUser);

            // arrange
            // 1. Create new update user notifications request with tags 
            var newUpdatedUserNotifications = new EditUserNotificationsRequestDto()
            {
                AutomaticallySubscribeToAllGroups = false,
                AutomaticallySubscribeToAllGroupsWithTag = true,
                SubscribedTagIds = new List<Guid>()
                {
                    Guid.Parse("caf77dee-a94f-49cb-b51f-e0c0e1067541"), Guid.Parse("bd456f08-f137-4382-8358-d52772c2dfc8")
                }
            };

            // act
            this._dataService.UpdateUserNotificationSettings(newUserId, newUpdatedUserNotifications);

            // assert
            var updatedNotificationSettings = this._dataService.GetNotificationSettingsForUserWithId(newUserId);

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
            this._dataService.UpdateUserNotificationSettings(userId, null);
        }

        [TestMethod]
        public void We_Can_Insert_New_Group()
        {
            // arrange
            var userId = Guid.NewGuid();
            var dbUser = new User()
            {
                Id = userId,
                NotificationSettings = null
            };

            var newAreaId = Guid.NewGuid();
            var newArea = new Area()
            {
                Id = newAreaId,
                Latitude = 10,
                Longitude = 10,
                Radius = RadiusRangeEnum.FiftyMeters
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Users = new FakeDbSet<User>() { dbUser },
                Areas = new FakeDbSet<Area>() { newArea }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.TagRepository).Returns(new TagRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));

            var newGroupRequest = new StandaloneGroupRequestDto()
            {
               Name = "New group",
               TagIds = new List<Guid>() { Guid.Parse("caf77dee-a94f-49cb-b51f-e0c0e1067541"), Guid.Parse("bd456f08-f137-4382-8358-d52772c2dfc8") },
               AreaIds = new List<Guid>() { newAreaId },
               UserId = userId
            };

            // act
            var newGroupId = this._dataService.InsertNewGroup(newGroupRequest);

            // assert
            Assert.IsTrue(newGroupId != Guid.Empty);

            var newlyCreatedGroup = this._dataService.GetGroupById(newGroupId);

            Assert.IsNotNull(newlyCreatedGroup);
        }

        [TestMethod]
        public void We_Can_Insert_Grou_As_New_Area()
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
            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.TagRepository).Returns(new TagRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));

            var newGroupRequest = new GroupAsNewAreaRequestDto()
            {
                Longitude = 10,
                Latitude = 10,
                Name = "Group 1",
                Radius = RadiusRangeEnum.FiftyMeters,
                TagIds =
                    new List<Guid>()
                    {
                        Guid.Parse("caf77dee-a94f-49cb-b51f-e0c0e1067541"),
                        Guid.Parse("bd456f08-f137-4382-8358-d52772c2dfc8")
                    },
                UserId = userId
            };

            var allAreasCount = this._dataService.GetAllAreas().Count();

            Assert.AreEqual(allAreasCount, 0);

            // act
            var newGroupId = this._dataService.InserGroupAsNewArea(newGroupRequest);

            // assert
            allAreasCount = this._dataService.GetAllAreas().Count();

            Assert.AreNotEqual(newGroupId, Guid.Empty);
            Assert.AreEqual(allAreasCount, 1);

            var newlyCreatedGroup = this._dataService.GetGroupById(newGroupId);
            Assert.IsNotNull(newlyCreatedGroup);
        }

        [TestMethod]
        public void We_Can_Get_AreaMetadata()
        {
            // arrange
            var myLocationLatitude = 10;
            var myLocationLongitude = 10;

            var groupOneId = Guid.NewGuid();
            var groupTwoId = Guid.NewGuid();

            var workingTag = new Tag()
            {
                Id = Guid.Parse("caf77dee-a94f-49cb-b51f-e0c0e1067541"),
                Name = "Help"
            };

            var groupOne = new Group()
            {
                Id = groupOneId, Name = "GroupOne", Users = new List<User>(),
                Tags = new List<Tag>() { workingTag }
            };
            var groupTwo = new Group() { Id = groupTwoId, Name = "GroupTwo", Users = new List<User>(), Tags = new List<Tag>() { workingTag } };

            var areaId = Guid.NewGuid();
            var area = new Area()
            {
                Id = areaId,
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne, groupTwo }
            };

            groupOne.Areas = new List<Area>() { area };
            groupTwo.Areas = new List<Area>() { area };

            var userId = Guid.NewGuid();
            var user = new User()
            {
                Id = userId,
                FirstName = "Stefcho",
                LastName = "Stefchev",
                Email = "mail@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString()
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo },
                Areas = new FakeDbSet<Area>() { area },
                Users = new FakeDbSet<User>() { user }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));

            // act
            var areaMetadata = this._dataService.GetMetadataForArea(myLocationLatitude, myLocationLongitude, areaId);

            // assert
            Assert.IsNotNull(areaMetadata);
            Assert.AreEqual(areaMetadata.TagIds.Count(), 1);
            Assert.AreEqual(areaMetadata.DistanceTo, 0);
        }

        [TestMethod]
        public void We_Can_Get_Metadata_For_Multiple_Areas()
        {
            // arrange
            var myLocationLatitude = 10;
            var myLocationLongitude = 10;

            var groupOneId = Guid.NewGuid();
            var groupTwoId = Guid.NewGuid();

            var workingTag = new Tag()
            {
                Id = Guid.Parse("caf77dee-a94f-49cb-b51f-e0c0e1067541"),
                Name = "Help"
            };

            var groupOne = new Group()
            {
                Id = groupOneId,
                Name = "GroupOne",
                Users = new List<User>(),
                Tags = new List<Tag>() { workingTag }
            };
            var groupTwo = new Group() { Id = groupTwoId, Name = "GroupTwo", Users = new List<User>(), Tags = new List<Tag>() { workingTag } };

            var firstAreaId = Guid.NewGuid();
            var firstArea = new Area()
            {
                Id = firstAreaId,
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne, groupTwo },
                Radius = RadiusRangeEnum.FiftyMeters
            };

            var secondAreaId = Guid.NewGuid();
            var secondArea = new Area()
            {
                Id = firstAreaId,
                Latitude = 10.00001,
                Longitude = 10.00001,
                Groups = new List<Group>() { groupOne, groupTwo },
                Radius = RadiusRangeEnum.FiveHundredMeters
            };

            groupOne.Areas = new List<Area>() { firstArea, secondArea };
            groupTwo.Areas = new List<Area>() { firstArea, secondArea };

            var userId = Guid.NewGuid();
            var user = new User()
            {
                Id = userId,
                FirstName = "Stefcho",
                LastName = "Stefchev",
                Email = "mail@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString()
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo },
                Areas = new FakeDbSet<Area>() { firstArea, secondArea },
                Users = new FakeDbSet<User>() { user }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));

            // act
            var multipleAreasMetadataRequestDto = new MultipleAreasMetadataRequestDto()
            {
                AreaIds = new List<Guid>() { firstAreaId, secondAreaId },
                Latitude = myLocationLatitude,
                Longitude = myLocationLongitude
            };

            var multipleAreasMetadata = this._dataService.GetMultipleAreasMetadata(multipleAreasMetadataRequestDto);

            // assert
            Assert.IsNotNull(multipleAreasMetadata);
            Assert.AreEqual(multipleAreasMetadata.Count(), 2);
            Assert.IsTrue(multipleAreasMetadata.Any(a => a.DistanceTo == 0));
        }

        [TestMethod]
        public void We_Can_Get_Users_From_Area()
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


            var groupOneId = Guid.NewGuid();
            var groupTwoId = Guid.NewGuid();

            var groupOne = new Group() { Id = groupOneId, Name = "GroupOne", Users = new List<User>() { firstUser } };
            var groupTwo = new Group() { Id = groupTwoId, Name = "GroupTwo", Users = new List<User>() { firstUser, secondUser } };

            var areaId = Guid.NewGuid();
            var area = new Area()
            {
                Id = areaId,
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne, groupTwo }
            };

            groupOne.Areas = new List<Area>() { area };
            groupTwo.Areas = new List<Area>() { area };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo },
                Areas = new FakeDbSet<Area>() { area },
                Users = new FakeDbSet<User>() { firstUser, secondUser }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));

            // act
            var usersFromArea = this._dataService.GetUsersFromArea(areaId);

            // assert
            Assert.IsNotNull(usersFromArea);

            var userIds = usersFromArea.Select(u => u.Id);

            Assert.IsTrue(userIds.Count() == userIds.Distinct().Count());
            Assert.AreEqual(userIds.Count(), 2);
        }

        [TestMethod]
        public void We_Can_Get_Areas_In_A_Radius_Of_Certain_Point()
        {
            // arrange
            var firstAreaId = Guid.NewGuid();
            var firstArea = new Area()
            {
                Id = firstAreaId,
                Latitude = 10,
                Longitude = 10,
                Radius = RadiusRangeEnum.FiftyMeters
            };

            var secondAreaId = Guid.NewGuid();
            var secondArea = new Area()
            {
                Id = secondAreaId,
                Latitude = 10.000001,
                Longitude = 10.000001,
                Radius = RadiusRangeEnum.FiftyMeters
            };

            var thirdAreaId = Guid.NewGuid();
            var thirdArea = new Area()
            {
                Id = thirdAreaId,
                Latitude = 42,
                Longitude = 42,
                Radius = RadiusRangeEnum.FiftyMeters
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Areas = new FakeDbSet<Area>() { firstArea, secondArea, thirdArea }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));

            // act
            var areasAroundCoordinatesInRadius = this._dataService.GetAreas(10, 10, 10);

            // assert
            Assert.IsNotNull(areasAroundCoordinatesInRadius);
            Assert.AreEqual(areasAroundCoordinatesInRadius.Count(), 2);

            var areaIds = areasAroundCoordinatesInRadius.Select(a => a.Id);
            Assert.IsTrue(areaIds.Contains(firstAreaId));
            Assert.IsTrue(areaIds.Contains(secondAreaId));
            Assert.IsFalse(areaIds.Contains(thirdAreaId));
        }

        [TestMethod]
        public void We_Can_Get_Areas_Inside_Of_Which_We_Are()
        {
            // arrange
            var firstAreaId = Guid.NewGuid();
            var firstArea = new Area()
            {
                Id = firstAreaId,
                Latitude = 10,
                Longitude = 10,
                Radius = RadiusRangeEnum.FiftyMeters
            };

            var secondAreaId = Guid.NewGuid();
            var secondArea = new Area()
            {
                Id = secondAreaId,
                Latitude = 10,
                Longitude = 10,
                Radius = RadiusRangeEnum.ThreeHundredMeters
            };

            var thirdAreaId = Guid.NewGuid();
            var thirdArea = new Area()
            {
                Id = thirdAreaId,
                Latitude = 50,
                Longitude = 50,
                Radius = RadiusRangeEnum.FiftyMeters
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Areas = new FakeDbSet<Area>() { firstArea, secondArea, thirdArea }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));

            // act
            var areasWhereWeFallIn = this._dataService.GetAreas(10, 10);

            // assert
            Assert.IsNotNull(areasWhereWeFallIn);
            Assert.AreEqual(areasWhereWeFallIn.Count(), 2);

            var areaIds = areasWhereWeFallIn.Select(a => a.Id);
            Assert.IsTrue(areaIds.Contains(firstAreaId));
            Assert.IsTrue(areaIds.Contains(secondAreaId));
            Assert.IsFalse(areaIds.Contains(thirdAreaId));
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
            var users = this._dataService.GetAllUsers();

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
            var user = this._dataService.GetUserByStsId(idsrvUniqueId);

            // assert
            Assert.IsNotNull(user);
        }

        [TestMethod]
        public void We_Can_Fetch_All_Users_From_Group()
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

            var groupId = Guid.NewGuid();
            var group = new Group() { Id = groupId, Name = "Group", Users = new List<User>() { firstUser, secondUser } };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { group }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));

            // act
            var allUsersInGroup = this._dataService.GetUsersFromGroup(groupId);

            // assert
            Assert.IsNotNull(allUsersInGroup);
            Assert.AreEqual(allUsersInGroup.Count(), 2);
        }

        [TestMethod]
        public void We_Can_Insert_New_Area()
        {
            // arrange
            var newAreaRequest = new NewAreaRequest()
            {
                Latitude = 10,
                Longitude = 10,
                Radius = RadiusRangeEnum.FiftyMeters
            };

            // act
            var newAreaId = this._dataService.InsertNewArea(newAreaRequest);

            // assert
            var newlyCreatedArea = this._dataService.GetAreaById(newAreaId);

            Assert.IsNotNull(newlyCreatedArea);
        }

        [TestMethod]
        public void We_Can_Get_All_Areas_A_Group_Belongs_To()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void Group_Gets_Deleted_If_No_Users_Belongs_To_It()
        {
            // arrange
            this._configurationWrapperMock.Setup(config => config.AutomaticallyCleanupGroupsAndAreas).Returns(true);

            var groupOneId = Guid.NewGuid();
            var groupTwoId = Guid.NewGuid();

            var groupOne = new Group() { Id = groupOneId, Name = "GroupOne", Users = new List<User>() };
            var groupTwo = new Group() { Id = groupTwoId, Name = "GroupTwo", Users = new List<User>() };

            var groupThreeId = Guid.NewGuid();
            var groupFourId = Guid.NewGuid();
            var groupThree = new Group() { Id = groupThreeId, Name = "GroupThree", Users = new List<User>() };
            var groupFour = new Group() { Id = groupFourId, Name = "GroupFour", Users = new List<User>() };

            var userId = Guid.NewGuid();
            var user = new User()
            {
                Id = userId,
                FirstName = "Stefcho",
                LastName = "Stefchev",
                Email = "mail@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString()
            };

            var area = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne, groupTwo, groupThree, groupFour }
            };

            groupOne.Areas = new List<Area>() { area };
            groupTwo.Areas = new List<Area>() { area };
            groupThree.Areas = new List<Area>() { area };
            groupFour.Areas = new List<Area>() { area };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo, groupThree, groupFour },
                Users = new FakeDbSet<User>() { user },
                Areas = new FakeDbSet<Area>() { area }
            }
            .Seed();

            DataGenerator.SetupAreaUserAndGroupRepositories(this._mockedLikkleUoW, populatedDatabase);

            var relateUserToGroupsRequest = new RelateUserToGroupsDto()
            {
                UserId = userId,
                Latitude = 10,
                Longitude = 10,
                GroupsUserSubscribes = new List<Guid>() { groupOneId, groupTwoId }
            };

            var relateUserToGroupsRequestWithUnsubscribedGroups = new RelateUserToGroupsDto()
            {
                UserId = userId,
                Latitude = 10,
                Longitude = 10,
                GroupsUserSubscribes = new List<Guid>() { groupThreeId, groupFourId }
            };

            // act and assert
            this._dataService.RelateUserToGroups(relateUserToGroupsRequest);
            
            Assert.AreEqual(4, this._dataService.AllGroups().Count());
            groupOne.Users.Add(user);
            groupTwo.Users.Add(user);
            Assert.IsTrue(this._dataService.AllGroups().Select(a => a.Id).Contains(groupOneId));
            Assert.IsTrue(this._dataService.AllGroups().Select(a => a.Id).Contains(groupTwoId));

            this._dataService.RelateUserToGroups(relateUserToGroupsRequestWithUnsubscribedGroups);

            Assert.AreEqual(2, this._dataService.AllGroups().Count());
            Assert.IsTrue(this._dataService.GetAllAreas().Any());
            Assert.IsFalse(this._dataService.AllGroups().Select(a => a.Id).Contains(groupOneId));
            Assert.IsFalse(this._dataService.AllGroups().Select(a => a.Id).Contains(groupTwoId));
        }

        [TestMethod]
        public void Area_Gets_Deleted_If_No_Groups_Belong_To_It()
        {
            // arrange
            this._configurationWrapperMock.Setup(config => config.AutomaticallyCleanupGroupsAndAreas).Returns(true);

            var groupOneId = Guid.NewGuid();
            var groupTwoId = Guid.NewGuid();

            var groupOne = new Group() { Id = groupOneId, Name = "GroupOne", Users = new List<User>() };
            var groupTwo = new Group() { Id = groupTwoId, Name = "GroupTwo", Users = new List<User>() };

            var userId = Guid.NewGuid();
            var user = new User()
            {
                Id = userId,
                FirstName = "Stefcho",
                LastName = "Stefchev",
                Email = "mail@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString()
            };

            var area = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne, groupTwo}
            };

            groupOne.Areas = new List<Area>() { area };
            groupTwo.Areas = new List<Area>() { area };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo},
                Users = new FakeDbSet<User>() { user },
                Areas = new FakeDbSet<Area>() { area }
            }
            .Seed();

            DataGenerator.SetupAreaUserAndGroupRepositories(this._mockedLikkleUoW, populatedDatabase);

            var relateUserToGroupsRequest = new RelateUserToGroupsDto()
            {
                UserId = userId,
                Latitude = 10,
                Longitude = 10,
                GroupsUserSubscribes = new List<Guid>() { groupOneId, groupTwoId }
            };

            var relateUserToGroupsRequestWithUnsubscribedGroups = new RelateUserToGroupsDto()
            {
                UserId = userId,
                Latitude = 10,
                Longitude = 10,
                GroupsUserSubscribes = new List<Guid>() { }
            };

            // act and assert
            this._dataService.RelateUserToGroups(relateUserToGroupsRequest);

            Assert.AreEqual(2, this._dataService.AllGroups().Count());
            groupOne.Users.Add(user);
            groupTwo.Users.Add(user);
            Assert.IsTrue(this._dataService.AllGroups().Select(a => a.Id).Contains(groupOneId));
            Assert.IsTrue(this._dataService.AllGroups().Select(a => a.Id).Contains(groupTwoId));

            this._mockedLikkleUoW.Setup(uow => uow.Save()).Callback((() =>
            {
                area.Groups = new List<Group>();
            }));

            this._dataService.RelateUserToGroups(relateUserToGroupsRequestWithUnsubscribedGroups);

            Assert.AreEqual(0, this._dataService.AllGroups().Count());
            Assert.IsTrue(!this._dataService.GetAllAreas().Any());
            Assert.IsFalse(this._dataService.AllGroups().Select(a => a.Id).Contains(groupOneId));
            Assert.IsFalse(this._dataService.AllGroups().Select(a => a.Id).Contains(groupTwoId));







            
        }
    }
}
