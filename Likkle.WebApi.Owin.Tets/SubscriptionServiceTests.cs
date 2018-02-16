using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessServices;
using Likkle.BusinessServices.Utils;
using Likkle.DataModel;
using Likkle.DataModel.Repositories;
using Likkle.DataModel.TestingPurposes;
using Likkle.DataModel.UnitOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Likkle.BusinessEntities.SignalrDtos;

namespace Likkle.WebApi.Owin.Tets
{
    [TestClass]
    public class SubscriptionServiceTests
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IGroupService _groupService;
        private readonly IUserService _userService;
        private readonly IAreaService _areaService;

        private readonly Mock<IAccelometerAlgorithmHelperService> _accelometerAlgorithmHelperService;

        private readonly Mock<ILikkleUoW> _mockedLikkleUoW;
        private readonly Mock<IConfigurationProvider> _mockedConfigurationProvider;
        private readonly Mock<IConfigurationWrapper> _configurationWrapperMock;
        private readonly Mock<ISubscriptionSettingsService> _subscriptionSettingsService;
        private readonly Mock<IGeoCodingManager> _geoCodingManagerMock;
        private readonly Mock<ISubscriptionService> _subscrServiceMock;
        private readonly Mock<ISignalrService> _signalrServiceMock;

        private readonly IEnumerable<Tag> _allTags;

        public SubscriptionServiceTests()
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

            this._accelometerAlgorithmHelperService = new Mock<IAccelometerAlgorithmHelperService>();
            this._subscriptionSettingsService = new Mock<ISubscriptionSettingsService>();

            var mapConfiguration = new MapperConfiguration(cfg => {
                cfg.AddProfile<EntitiesMappingProfile>();
            });
            this._mockedConfigurationProvider.Setup(mc => mc.CreateMapper()).Returns(mapConfiguration.CreateMapper);

            _geoCodingManagerMock = new Mock<IGeoCodingManager>();
            _geoCodingManagerMock.Setup(gcm => gcm.GetApproximateAddress(It.IsAny<NewAreaRequest>()))
                .Returns(Guid.NewGuid().ToString);

            _subscrServiceMock = new Mock<ISubscriptionService>();
            _subscrServiceMock.Setup(ssm => ssm.AutoSubscribeUsersFromExistingAreas(It.IsAny<IEnumerable<Guid>>(), It.IsAny<StandaloneGroupRequestDto>(), It.IsAny<Guid>(), It.IsAny<Guid>()));

            _signalrServiceMock = new Mock<ISignalrService>();

            this._configurationWrapperMock = new Mock<IConfigurationWrapper>();

            _allTags = this._mockedLikkleUoW.Object.TagRepository.GetAllTags().ToList();

            this._subscriptionService = new SubscriptionService(
                this._mockedLikkleUoW.Object,
                this._mockedConfigurationProvider.Object,
                this._configurationWrapperMock.Object,
                this._signalrServiceMock.Object);

            this._groupService = new GroupService(
                this._mockedLikkleUoW.Object,
                this._mockedConfigurationProvider.Object,
                this._configurationWrapperMock.Object,
                this._geoCodingManagerMock.Object,
                this._subscrServiceMock.Object);

            this._userService = new UserService(
                this._mockedLikkleUoW.Object,
                this._mockedConfigurationProvider.Object,
                this._configurationWrapperMock.Object,
                this._accelometerAlgorithmHelperService.Object,
                this._subscriptionSettingsService.Object,
                this._subscrServiceMock.Object);

            this._areaService = new AreaService(
                this._mockedLikkleUoW.Object,
                this._mockedConfigurationProvider.Object,
                this._configurationWrapperMock.Object,
                null);
        }

        [TestMethod]
        public void We_Can_Relate_User_To_Groups()
        {
            // arrange
            var groupOneId = Guid.NewGuid();
            var groupTwoId = Guid.NewGuid();

            var groupOne = new Group() { Id = groupOneId, Name = "GroupOne", Users = new List<User>(), IsActive = true};
            var groupTwo = new Group() { Id = groupTwoId, Name = "GroupTwo", Users = new List<User>(), IsActive = true };

            var area = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne, groupTwo },
                IsActive = true
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

            var firstHistoryGroup = new HistoryGroup()
            {
                DateTimeGroupWasSubscribed = DateTime.UtcNow,
                GroupId = groupOneId,
                GroupThatWasPreviouslySubscribed = groupOne,
                UserId = userId,
                UserWhoSubscribedGroup = user,
                Id = Guid.NewGuid()
            };

            var secondHistoryGroup = new HistoryGroup()
            {
                DateTimeGroupWasSubscribed = DateTime.UtcNow,
                GroupId = groupTwoId,
                GroupThatWasPreviouslySubscribed = groupTwo,
                UserId = userId,
                UserWhoSubscribedGroup = user,
                Id = Guid.NewGuid()
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo },
                Areas = new FakeDbSet<Area>() { area },
                Users = new FakeDbSet<User>() { user },
                HistoryGroups = new FakeDbSet<HistoryGroup>() { firstHistoryGroup, secondHistoryGroup }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.HistoryGroupRepository).Returns(new HistoryGroupRepository(populatedDatabase));

            var relateUserToGroupsRequest = new RelateUserToGroupsDto()
            {
                UserId = userId,
                Latitude = 10,
                Longitude = 10,
                GroupsUserSubscribes = new List<Guid>() { groupOneId, groupTwoId }
            };

            // act
            this._subscriptionService.RelateUserToGroups(relateUserToGroupsRequest);
            var userSubscribtionsAroundCoordintes = this._groupService.GetUserSubscriptions(userId, 10, 10);

            // assert subscription works
            Assert.IsNotNull(userSubscribtionsAroundCoordintes);
            Assert.AreEqual(userSubscribtionsAroundCoordintes.Count(), 2);
            Assert.IsTrue(userSubscribtionsAroundCoordintes.Contains(groupOneId));
            Assert.IsTrue(userSubscribtionsAroundCoordintes.Contains(groupTwoId));
            Assert.IsNotNull(user.HistoryGroups);
            Assert.AreEqual(user.HistoryGroups.Count(), 2);
            var historyGroupIds = user.HistoryGroups.Select(hgr => hgr.GroupId).ToList();
            Assert.IsTrue(historyGroupIds.Contains(groupOneId) && historyGroupIds.Contains(groupTwoId));

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
            user.HistoryGroups = new List<HistoryGroup>() {firstHistoryGroup, secondHistoryGroup};

            // act
            this._subscriptionService.RelateUserToGroups(relateUserToGroupsRequest);
            userSubscribtionsAroundCoordintes = this._groupService.GetUserSubscriptions(userId, 10, 10);

            // assert unsubscription works
            Assert.IsNotNull(userSubscribtionsAroundCoordintes);
            Assert.AreEqual(userSubscribtionsAroundCoordintes.Count(), 1);
            Assert.IsFalse(userSubscribtionsAroundCoordintes.Contains(groupOneId));
            Assert.IsTrue(userSubscribtionsAroundCoordintes.Contains(groupTwoId));
            Assert.IsTrue(user.HistoryGroups != null);
            Assert.IsTrue(user.HistoryGroups.Select(hgr => hgr.GroupId).Contains(groupTwoId));
            Assert.IsTrue(this._mockedLikkleUoW.Object.HistoryGroupRepository.AllHistoryGroups().Count() == 2);

            // arrange
            var groupThree = new Group() { Id = Guid.NewGuid(), Name = "GroupThree", Users = new List<User>(), IsActive = true };
            var groupFour = new Group() { Id = Guid.NewGuid(), Name = "GroupFour", Users = new List<User>(), IsActive = true };

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
            this._subscriptionService.RelateUserToGroups(relateUserToGroupsRequest);
            userSubscribtionsAroundCoordintes = this._groupService.GetUserSubscriptions(userId, 10, 10);

            // assert
            Assert.IsNotNull(userSubscribtionsAroundCoordintes);
            Assert.AreEqual(userSubscribtionsAroundCoordintes.Count(), 2);
            Assert.IsFalse(userSubscribtionsAroundCoordintes.Contains(groupOneId));
            Assert.IsFalse(userSubscribtionsAroundCoordintes.Contains(groupTwoId));
            Assert.IsTrue(userSubscribtionsAroundCoordintes.Contains(groupThree.Id));
            Assert.IsTrue(userSubscribtionsAroundCoordintes.Contains(groupFour.Id));
        }

        [TestMethod]
        public void Empty_List_Is_Returned_When_No_Available_Groups_For_The_User_Are_Present_When_Getting_Subscriptions()
        {
            // arrange
            var groupOneId = Guid.NewGuid();
            var groupOne = new Group() { Id = groupOneId, Name = "GroupOne", Users = new List<User>(), IsActive = true };

            var area = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne },
                IsActive = true
            };

            groupOne.Areas = new List<Area>() { area };

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
                Groups = new FakeDbSet<Group>() { groupOne },
                Areas = new FakeDbSet<Area>() { area },
                Users = new FakeDbSet<User>() { user }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));

            // act
            var userSubscribtionsAroundCoordintes = this._groupService.GetUserSubscriptions(userId, 10, 10);

            // assert
            Assert.IsNotNull(userSubscribtionsAroundCoordintes);
            Assert.IsTrue(!userSubscribtionsAroundCoordintes.Any());
        }

        [TestMethod]
        public void When_User_Location_Is_Changed_Group_Is_Removed_But_HistoryGroup_Stays()
        {
            // arrange
            var groupOneId = Guid.NewGuid();
            var groupTwoId = Guid.NewGuid();

            var groupOne = new Group() { Id = groupOneId, Name = "GroupOne", Users = new List<User>() };
            var groupTwo = new Group() { Id = groupTwoId, Name = "GroupTwo", Users = new List<User>() };

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
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo },
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
            this._subscriptionService.RelateUserToGroups(relateUserToGroupsRequest);

            Assert.IsNotNull(user.Groups);
            Assert.AreEqual(user.Groups.Count(), 2);

            Assert.IsNotNull(user.HistoryGroups);
            Assert.AreEqual(user.HistoryGroups.Count(), 2);

            this._userService.UpdateUserLocation(userId, 90, 90);
            Assert.AreEqual(user.Groups.Count(), 0);
            Assert.AreEqual(user.HistoryGroups.Count(), 2);
        }
        
        // TODO: Extract common parts from this method and the one below
        [TestMethod]
        public void Group_Gets_Inactive_When_No_Users_Belong_To_It()
        {
            // arrange
            this._configurationWrapperMock.Setup(config => config.AutomaticallyCleanupGroupsAndAreas).Returns(true);

            var userId = Guid.NewGuid();
            var user = new User()
            {
                Id = userId,
                FirstName = "Stefcho",
                LastName = "Stefchev",
                Email = "mail@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString()
            };

            var groupOneId = Guid.NewGuid();
            var groupTwoId = Guid.NewGuid();

            var groupOne = new Group() { Id = groupOneId, Name = "GroupOne", Users = new List<User>() { user }, IsActive = true };
            var groupTwo = new Group() { Id = groupTwoId, Name = "GroupTwo", Users = new List<User>(), IsActive = true };

            var areaId = Guid.NewGuid();
            var area = new Area()
            {
                Id = areaId,
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne, groupTwo },
                IsActive = true
            };

            groupOne.Areas = new List<Area>() { area };
            groupTwo.Areas = new List<Area>() { area };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo },
                Users = new FakeDbSet<User>() { user },
                Areas = new FakeDbSet<Area>() { area }
            }
            .Seed();
            DataGenerator.SetupAreaUserAndGroupRepositories(this._mockedLikkleUoW, populatedDatabase);

            // act 
            var relateUserToGroupsRequest = new RelateUserToGroupsDto()
            {
                UserId = userId,
                Latitude = 10,
                Longitude = 10,
                GroupsUserSubscribes = new List<Guid>() { groupTwoId }
            };

            this._subscriptionService.RelateUserToGroups(relateUserToGroupsRequest);

            // assert
            Assert.AreEqual(2, this._areaService.GetAreaById(areaId).Groups.Count());
            Assert.IsFalse(this._mockedLikkleUoW.Object.GroupRepository.GetGroupById(groupOneId).IsActive);
            Assert.IsTrue(this._mockedLikkleUoW.Object.GroupRepository.GetGroupById(groupTwoId).IsActive);
        }

        [TestMethod]
        public void Area_Gets_Inactive_When_No_Active_Groups_Belong_To_It()
        {
            // arrange
            this._configurationWrapperMock.Setup(config => config.AutomaticallyCleanupGroupsAndAreas).Returns(true);

            var userId = Guid.NewGuid();
            var user = new User()
            {
                Id = userId,
                FirstName = "Stefcho",
                LastName = "Stefchev",
                Email = "mail@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString()
            };

            var groupOneId = Guid.NewGuid();
            var groupTwoId = Guid.NewGuid();

            var groupOne = new Group() { Id = groupOneId, Name = "GroupOne", Users = new List<User>() { user }, IsActive = true };
            var groupTwo = new Group() { Id = groupTwoId, Name = "GroupTwo", Users = new List<User>(), IsActive = false };

            var areaId = Guid.NewGuid();
            var area = new Area()
            {
                Id = areaId,
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne, groupTwo },
                IsActive = true
            };

            groupOne.Areas = new List<Area>() { area };
            groupTwo.Areas = new List<Area>() { area };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo },
                Users = new FakeDbSet<User>() { user },
                Areas = new FakeDbSet<Area>() { area }
            }
            .Seed();
            DataGenerator.SetupAreaUserAndGroupRepositories(this._mockedLikkleUoW, populatedDatabase);

            // act 
            var relateUserToGroupsRequest = new RelateUserToGroupsDto()
            {
                UserId = userId,
                Latitude = 10,
                Longitude = 10,
                GroupsUserSubscribes = new List<Guid>() { groupTwoId }
            };

            this._subscriptionService.RelateUserToGroups(relateUserToGroupsRequest);

            // assert
            Assert.AreEqual(2, this._areaService.GetAreaById(areaId).Groups.Count());

            Assert.IsFalse(this._mockedLikkleUoW.Object.GroupRepository.GetGroupById(groupOneId).IsActive);
            Assert.IsFalse(this._mockedLikkleUoW.Object.GroupRepository.GetGroupById(groupTwoId).IsActive);

            Assert.IsFalse(this._mockedLikkleUoW.Object.AreaRepository.GetAreaById(areaId).IsActive);
        }

        [TestMethod]
        public void We_Can_AutoSubscribe_UsersFromExistingAreas()
        {
            // arrange
            var userOneId = Guid.NewGuid();
            var userOne = new User()
            {
                Id = userOneId,
                FirstName = "Stefcho",
                LastName = "Stefchev",
                Email = "mail@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true, AutomaticallySubscribeToAllGroupsWithTag = false
                }
            };

            var userTwoId = Guid.NewGuid();
            var userTwo = new User()
            {
                Id = userTwoId,
                FirstName = "Ralph",
                LastName = "Lauren",
                Email = "rlauren@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = false,
                    AutomaticallySubscribeToAllGroupsWithTag = true,
                    Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList()
                }
            };

            var groupOneId = Guid.NewGuid();
            var groupTwoId = Guid.NewGuid();
            var groupThreeId = Guid.NewGuid();

            var groupOne = new Group() { Id = groupOneId, Name = "GroupOne", Users = new List<User>() { userOne, userTwo }, IsActive = true };
            var groupTwo = new Group() { Id = groupTwoId, Name = "GroupTwo", Users = new List<User>() { userTwo}, IsActive = true };
            var groupThree = new Group() { Id = groupThreeId, Name = "GroupThree", Users = new List<User>() { }, IsActive = true };

            userOne.Groups = new List<Group>() { groupOne };
            userTwo.Groups = new List<Group>() { groupOne, groupTwo };

            var areaId = Guid.NewGuid();
            var area = new Area()
            {
                Id = areaId,
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne, groupTwo },
                IsActive = true
            };

            groupOne.Areas = new List<Area>() { area };
            groupTwo.Areas = new List<Area>() { area };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo, groupThree },
                Users = new FakeDbSet<User>() { userOne, userTwo },
                Areas = new FakeDbSet<Area>() { area }
            }
            .Seed();
            DataGenerator.SetupAreaUserAndGroupRepositories(this._mockedLikkleUoW, populatedDatabase);

            // act
            this._subscriptionService.AutoSubscribeUsersFromExistingAreas(
                new List<Guid>() { areaId }, 
                new StandaloneGroupRequestDto() { TagIds = _allTags.Where(t => t.Name == "Sport").Select(t => t.Id).ToList() },
                groupThreeId,
                Guid.NewGuid());

            // assert
            Assert.IsTrue(userOne.Groups.Select(gr => gr.Id).Contains(groupOneId));
            Assert.IsTrue(userOne.Groups.Select(gr => gr.Id).Contains(groupThreeId));

            Assert.IsTrue(userTwo.Groups.Select(gr => gr.Id).Contains(groupOneId));
            Assert.IsTrue(userTwo.Groups.Select(gr => gr.Id).Contains(groupTwoId));
            Assert.IsTrue(userTwo.Groups.Select(gr => gr.Id).Contains(groupThreeId));
        }

        [TestMethod]
        public void We_Can_AutoSubscribe_UsersForGroupAsNewArea()
        {
            // arrange
            var userOneId = Guid.NewGuid();
            var userOne = new User()
            {
                Id = userOneId,
                FirstName = "Stefcho",
                LastName = "Stefchev",
                Email = "mail@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Latitude = 10.000001,
                Longitude = 10.000001,
                Groups = new List<Group>() { }
            };

            var userTwoId = Guid.NewGuid();
            var userTwo = new User()
            {
                Id = userTwoId,
                FirstName = "Ralph",
                LastName = "Lauren",
                Email = "rlauren@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = false,
                    AutomaticallySubscribeToAllGroupsWithTag = true,
                    Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList()
                },
                Latitude = 10.000002,
                Longitude = 10.000002,
                Groups = new List<Group>() { }
            };

            var userThreeId = Guid.NewGuid();
            var userThree = new User()
            {
                Id = userThreeId,
                FirstName = "Rudolf",
                LastName = "Raindeer",
                Email = "rsdsdsen@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = false,
                    AutomaticallySubscribeToAllGroupsWithTag = true,
                    Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList()
                },
                Latitude = 45.000002,
                Longitude = 120.000002,
                Groups = new List<Group>() { }
            };

            var groupThreeId = Guid.NewGuid();
            var groupThree = new Group() { Id = groupThreeId, Name = "GroupThree", Users = new List<User>() { }, IsActive = true, Tags  = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Users = new FakeDbSet<User>() { userOne, userTwo, userThree },
                Groups = new FakeDbSet<Group>() { groupThree }
            }
            .Seed();
            DataGenerator.SetupUserAndGroupRepositories(this._mockedLikkleUoW, populatedDatabase);

            // act
            this._subscriptionService.AutoSubscribeUsersForGroupAsNewArea(Guid.NewGuid(), 10.000000, 10.000000, BusinessEntities.Enums.RadiusRangeEnum.FiftyMeters, groupThreeId, Guid.NewGuid());

            // assert
            Assert.IsTrue(userOne.Groups.Contains(groupThree));
            Assert.IsTrue(userTwo.Groups.Contains(groupThree));
            Assert.IsFalse(userThree.Groups.Contains(groupThree));
        }

        [TestMethod]
        public void We_Can_AutoSubscribe_UsersForGroupAsNewArea_And_Ping_Correct_Set_Of_Users_Via_SignalR()
        {
            // arrange
            var userOneId = Guid.NewGuid();
            var userOne = new User()
            {
                Id = userOneId,
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = false,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Latitude = 10.000001,
                Longitude = 10.000001,
                Groups = new List<Group>() { }
            };

            var userTwoId = Guid.NewGuid();
            var userTwo = new User()
            {
                Id = userTwoId,
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = false,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Latitude = 10.000002,
                Longitude = 10.000002,
                Groups = new List<Group>() { }
            };

            var userThreeId = Guid.NewGuid();
            var userThree = new User()
            {
                Id = userThreeId,
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = false,
                    AutomaticallySubscribeToAllGroupsWithTag = true,
                    Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList()
                },
                Latitude = 10.000003,
                Longitude = 10.000003,
                Groups = new List<Group>() { }
            };

            var userFourId = Guid.NewGuid();
            var userFour = new User()
            {
                Id = userFourId,
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Latitude = 10.000004,
                Longitude = 10.000004,
                Groups = new List<Group>() { }
            };

            var groupId = Guid.NewGuid();
            var group = new Group() {
                Id = groupId,
                Users = new List<User>() { },
                IsActive = true,
                Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList()
            };

            var areaId = Guid.NewGuid();
            var area = new Area()
            {
                Id = areaId,
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { },
                IsActive = true,
                Radius = BusinessEntities.Enums.RadiusRangeEnum.HunderdAndFiftyMeters
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { group },
                Users = new FakeDbSet<User>() { userOne, userTwo, userThree, userFour },
                Areas = new FakeDbSet<Area>() { area }
            }
            .Seed();
            DataGenerator.SetupAreaUserAndGroupRepositories(this._mockedLikkleUoW, populatedDatabase);

            // act
            this._subscriptionService.AutoSubscribeUsersForGroupAsNewArea(
                area.Id, 
                area.Latitude, 
                area.Longitude, 
                area.Radius, 
                groupId, 
                userOne.Id);

            // assert
            _signalrServiceMock.Verify(m => m.GroupAsNewAreaWasCreatedAroundMe(
                It.IsIn<string>(new string[] { userTwoId.ToString(), userThreeId.ToString(), userFourId.ToString() }),
                It.IsAny<SRAreaDto>(), 
                It.IsAny<SRGroupDto>(), 
                It.IsAny<bool>()), 
                Times.Exactly(3));
        }

        [TestMethod]
        public void We_Can_AutoSubscribe_UsersForRecreatedGroup()
        {
            // arrange
            var userOneId = Guid.NewGuid();
            var userOne = new User()
            {
                Id = userOneId,
                FirstName = "Stefcho",
                LastName = "Stefchev",
                Email = "mail@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Latitude = 10.000001,
                Longitude = 10.000001,
                Groups = new List<Group>() { }
            };
            
            var groupOneId = Guid.NewGuid();
            var groupOne = new Group() { Id = groupOneId, Name = "GroupOne", Users = new List<User>() { }, IsActive = false, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };

            var areaId = Guid.NewGuid();
            var area = new Area()
            {
                Id = areaId,
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne },
                IsActive = false,
                Radius = BusinessEntities.Enums.RadiusRangeEnum.FiftyMeters
            };

            groupOne.Areas = new List<Area>() { area };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne },
                Users = new FakeDbSet<User>() { userOne },
                Areas = new FakeDbSet<Area>() { area }
            }
            .Seed();
            DataGenerator.SetupAreaUserAndGroupRepositories(this._mockedLikkleUoW, populatedDatabase);

            // act
            this._subscriptionService.AutoSubscribeUsersForRecreatedGroup(new List<Guid>() { areaId }, groupOneId, Guid.NewGuid());

            // assert
            Assert.IsTrue(userOne.Groups.Contains(groupOne));
        }

        [TestMethod]
        public void When_AutoSubscribe_UsersFromExistingAreas_User_Who_Fired_The_Action_Does_Not_Get_Notified()
        {
            // arrange
            var userOneId = Guid.NewGuid();
            var userOne = new User()
            {
                Id = userOneId,
                FirstName = "Stefcho",
                LastName = "Stefchev",
                Email = "mail@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Latitude = 10.000001,
                Longitude = 10.000001,
                Groups = new List<Group>() { }
            };

            var userTwoId = Guid.NewGuid();
            var userTwo = new User()
            {
                Id = userTwoId,
                FirstName = "Fires",
                LastName = "Fires",
                Email = "mal@maigtgtl.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Latitude = 10.000001,
                Longitude = 10.000001,
                Groups = new List<Group>() { }
            };

            var userThreeId = Guid.NewGuid();
            var userThree = new User()
            {
                Id = userThreeId,
                FirstName = "Three",
                LastName = "Three",
                Email = "masl@maigtgtsl.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Latitude = 10.000001,
                Longitude = 10.000001,
                Groups = new List<Group>() { }
            };

            var groupOneId = Guid.NewGuid();
            var groupOne = new Group() { Id = groupOneId, Name = "GroupOne", Users = new List<User>() { userOne, userTwo, userThree }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };

            var groupTwoId = Guid.NewGuid();
            var groupTwo = new Group() { Id = groupTwoId, Name = "GroupTwo", Users = new List<User>() { }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };

            userOne.Groups.Add(groupOne);
            userTwo.Groups.Add(groupOne);
            userThree.Groups.Add(groupOne);

            var areaId = Guid.NewGuid();
            var area = new Area()
            {
                Id = areaId,
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne },
                IsActive = false,
                Radius = BusinessEntities.Enums.RadiusRangeEnum.FiftyMeters
            };

            groupOne.Areas = new List<Area>() { area };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo },
                Users = new FakeDbSet<User>() { userOne, userTwo, userThree },
                Areas = new FakeDbSet<Area>() { area }
            }
           .Seed();
            DataGenerator.SetupAreaUserAndGroupRepositories(this._mockedLikkleUoW, populatedDatabase);

            // act
            this._subscriptionService.AutoSubscribeUsersFromExistingAreas(new List<Guid>() { areaId }, new StandaloneGroupRequestDto() { }, groupTwoId, userOneId);

            // assert
            this._signalrServiceMock.Verify(srs => srs.GroupAttachedToExistingAreasWasCreatedAroundMe(It.IsAny<string>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<SRGroupDto>(), It.IsAny<bool>()), Times.Exactly(2));
        }

        [TestMethod]
        public void When_AutoSubscribe_UsersForGroupAsNewArea_User_Who_Fired_The_Action_Does_Not_Get_Notified()
        {
            // arrange
            var userOneId = Guid.NewGuid();
            var userOne = new User()
            {
                Id = userOneId,
                FirstName = "Stefcho",
                LastName = "Stefchev",
                Email = "mail@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Latitude = 10.000001,
                Longitude = 10.000001,
                Groups = new List<Group>() { }
            };

            var userTwoId = Guid.NewGuid();
            var userTwo = new User()
            {
                Id = userTwoId,
                FirstName = "Ralph",
                LastName = "Lauren",
                Email = "Rasss@ta.fari",
                IdsrvUniqueId = Guid.NewGuid().ToString(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = false,
                    AutomaticallySubscribeToAllGroupsWithTag = true,
                    Tags = _allTags.Where(t => t.Name == "Sport").ToList()
                },
                Latitude = 10.000002,
                Longitude = 10.000002,
                Groups = new List<Group>() { }
            };

            var groupOneId = Guid.NewGuid();
            var groupOne = new Group() { Id = groupOneId, Name = "GroupOne", Users = new List<User>() { userOne }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };

            var areaId = Guid.NewGuid();
            var area = new Area()
            {
                Id = areaId,
                Latitude = 10.000001,
                Longitude = 10.000001,
                Groups = new List<Group>() { groupOne },
                IsActive = false,
                Radius = BusinessEntities.Enums.RadiusRangeEnum.FiftyMeters
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne },
                Users = new FakeDbSet<User>() { userOne, userTwo },
                Areas = new FakeDbSet<Area>() { area }
            }
           .Seed();
            DataGenerator.SetupAreaUserAndGroupRepositories(this._mockedLikkleUoW, populatedDatabase);

            // act
            this._subscriptionService.AutoSubscribeUsersForGroupAsNewArea(areaId, area.Latitude, area.Longitude, area.Radius, groupOneId, userOneId);

            // assert
            this._signalrServiceMock.Verify(srs => srs.GroupAsNewAreaWasCreatedAroundMe(It.IsAny<string>(), It.IsAny<SRAreaDto>(), It.IsAny<SRGroupDto>(), It.IsAny<bool>()), Times.Once);
        }

        [TestMethod]
        public void When_AutoSubscribe_UsersForRecreatedGroup_User_Who_Fired_The_Action_Does_Not_Get_Notified()
        {
            // arrange
            var groupOneId = Guid.NewGuid();
            var groupOne = new Group() { Id = groupOneId, Name = "GroupOne", Users = new List<User>() { }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };

            var areaOneId = Guid.NewGuid();
            var areaOne = new Area()
            {
                Id = areaOneId,
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne },
                IsActive = true,
                Radius = BusinessEntities.Enums.RadiusRangeEnum.FiftyMeters
            };

            var areaTwoId = Guid.NewGuid();
            var areaTwo = new Area()
            {
                Id = areaTwoId,
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne },
                IsActive = true,
                Radius = BusinessEntities.Enums.RadiusRangeEnum.HunderdAndFiftyMeters
            };

            groupOne.Areas = new List<Area>() { areaOne, areaTwo };

            var userOneId = Guid.NewGuid();
            var userOne = new User()
            {
                Id = userOneId,
                FirstName = "Stefcho",
                LastName = "Stefchev",
                Email = "mail@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Latitude = 10.000001,
                Longitude = 10.000001,
                Groups = new List<Group>() { }
            };
            
            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne },
                Users = new FakeDbSet<User>() { userOne },
                Areas = new FakeDbSet<Area>() { areaOne, areaTwo }
            }
            .Seed();
            DataGenerator.SetupAreaUserAndGroupRepositories(this._mockedLikkleUoW, populatedDatabase);

            // act
            this._subscriptionService.AutoSubscribeUsersForRecreatedGroup(new List<Guid>() { areaOneId, areaTwoId }, groupOneId, Guid.NewGuid());

            // assert
            this._signalrServiceMock.Verify(srs => srs.GroupAroundMeWasRecreated(It.IsAny<string>(), It.IsAny<IEnumerable<SRAreaDto>>(), It.IsAny<SRGroupDto>(), It.IsAny<bool>()), Times.Once);
        }

        [TestMethod]
        public void When_AutoIncreaseUsersInGroups_Is_Called_User_Who_Fired_The_Action_Does_Not_Get_Notified()
        {
            // arrange
            var groupOneId = Guid.NewGuid();
            var groupOne = new Group() { Id = groupOneId, Name = "GroupOne", Users = new List<User>() { }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };

            var groupTwoId = Guid.NewGuid();
            var groupTwo = new Group() { Id = groupTwoId, Name = "GroupTwo", Users = new List<User>() { }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };

            var groupThreeId = Guid.NewGuid();
            var groupThree = new Group() { Id = groupThreeId, Name = "GroupThree", Users = new List<User>() { }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };

            var areaOneId = Guid.NewGuid();
            var areaOne = new Area()
            {
                Id = areaOneId,
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne, groupTwo, groupThree },
                IsActive = true,
                Radius = BusinessEntities.Enums.RadiusRangeEnum.FiftyMeters
            };

            groupOne.Areas = new List<Area>() { areaOne };
            groupTwo.Areas = new List<Area>() { areaOne };

            var userOneId = Guid.NewGuid();
            var userOne = new User()
            {
                Id = userOneId,
                FirstName = "Stefcho",
                LastName = "Stefchev",
                Email = "mail@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Groups = new List<Group>() { groupOne, groupTwo }
            };

            var userTwoId = Guid.NewGuid();
            var userTwo = new User()
            {
                Id = userTwoId,
                FirstName = "Ralph",
                LastName = "Lauren",
                Email = "mailss@ssmail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Groups = new List<Group>() { groupOne }
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo, groupThree },
                Users = new FakeDbSet<User>() { userOne, userTwo },
                Areas = new FakeDbSet<Area>() { areaOne }
            }
            .Seed();
            DataGenerator.SetupAreaUserAndGroupRepositories(this._mockedLikkleUoW, populatedDatabase);

            // act
            this._subscriptionService.AutoIncreaseUsersInGroups(new List<Guid>() { groupThreeId }, userOneId);

            // assert
            this._signalrServiceMock.Verify(srs => srs.GroupWasJoinedByUser(groupThreeId, It.IsAny<List<string>>()), Times.Once);
        }

        [TestMethod]
        public void When_AutoDecreaseUsersInGroups_Is_Called_User_Who_Fired_The_Action_Does_Not_Get_Notified()
        {
            // arrange
            var groupOneId = Guid.NewGuid();
            var groupOne = new Group() { Id = groupOneId, Name = "GroupOne", Users = new List<User>() { }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };

            var groupTwoId = Guid.NewGuid();
            var groupTwo = new Group() { Id = groupTwoId, Name = "GroupTwo", Users = new List<User>() { }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };

            var areaOneId = Guid.NewGuid();
            var areaOne = new Area()
            {
                Id = areaOneId,
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne, groupTwo },
                IsActive = true,
                Radius = BusinessEntities.Enums.RadiusRangeEnum.FiftyMeters
            };

            groupOne.Areas = new List<Area>() { areaOne };
            groupTwo.Areas = new List<Area>() { areaOne };

            var userOneId = Guid.NewGuid();
            var userOne = new User()
            {
                Id = userOneId,
                FirstName = "Stefcho",
                LastName = "Stefchev",
                Email = "mail@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Groups = new List<Group>() { groupOne, groupTwo }
            };

            var userTwoId = Guid.NewGuid();
            var userTwo = new User()
            {
                Id = userTwoId,
                FirstName = "Ralph",
                LastName = "Lauren",
                Email = "mailss@ssmail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Groups = new List<Group>() { groupOne }
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo },
                Users = new FakeDbSet<User>() { userOne, userTwo },
                Areas = new FakeDbSet<Area>() { areaOne }
            }
            .Seed();
            DataGenerator.SetupAreaUserAndGroupRepositories(this._mockedLikkleUoW, populatedDatabase);

            // act
            this._subscriptionService.AutoDecreaseUsersInGroups(new List<Guid>() { groupTwoId }, userOneId);

            // assert
            this._signalrServiceMock.Verify(srs => srs.GroupWasLeftByUser(groupTwoId, It.IsAny<List<string>>()), Times.Once);
        }
    }
}
