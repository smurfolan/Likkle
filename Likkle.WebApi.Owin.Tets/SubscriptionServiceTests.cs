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
        public void RelateUserToGroups_We_Can_Relate_User_To_Groups()
        {
            // arrange
            var groupOne = new Group() { Id = Guid.NewGuid(), Users = new List<User>(), IsActive = true };
            var groupTwo = new Group() { Id = Guid.NewGuid(), Users = new List<User>(), IsActive = true };

            var area = new Area()
            {
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne, groupTwo }
            };

            groupOne.Areas = new List<Area>() { area };
            groupTwo.Areas = new List<Area>() { area };

            var user = new User() { };

            var firstHistoryGroup = new HistoryGroup()
            {
                GroupId = groupOne.Id,
                UserWhoSubscribedGroup = user
            };

            var secondHistoryGroup = new HistoryGroup()
            {
                GroupId = groupTwo.Id,
                UserWhoSubscribedGroup = user
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
                UserId = user.Id,
                Latitude = 10,
                Longitude = 10,
                GroupsUserSubscribes = new List<Guid>() { groupOne.Id, groupTwo.Id }
            };

            // act
            this._subscriptionService.RelateUserToGroups(relateUserToGroupsRequest);
            var userSubscribtionsAroundCoordintes = this._groupService.GetUserSubscriptions(user.Id, 10, 10);

            // assert subscription works
            Assert.IsNotNull(userSubscribtionsAroundCoordintes);
            Assert.AreEqual(userSubscribtionsAroundCoordintes.Count(), 2);
            Assert.IsTrue(userSubscribtionsAroundCoordintes.Contains(groupOne.Id));
            Assert.IsTrue(userSubscribtionsAroundCoordintes.Contains(groupTwo.Id));
            Assert.IsNotNull(user.HistoryGroups);
            Assert.AreEqual(user.HistoryGroups.Count(), 2);
            var historyGroupIds = user.HistoryGroups.Select(hgr => hgr.GroupId).ToList();
            Assert.IsTrue(historyGroupIds.Contains(groupOne.Id) && historyGroupIds.Contains(groupTwo.Id));

            // arrange
            relateUserToGroupsRequest = new RelateUserToGroupsDto()
            {
                UserId = user.Id,
                Latitude = 10,
                Longitude = 10,
                GroupsUserSubscribes = new List<Guid>() { groupTwo.Id }
            };

            groupOne.Users = new List<User>() { user };
            groupTwo.Users = new List<User>() { user };
            user.HistoryGroups = new List<HistoryGroup>() { firstHistoryGroup, secondHistoryGroup };

            // act
            this._subscriptionService.RelateUserToGroups(relateUserToGroupsRequest);
            userSubscribtionsAroundCoordintes = this._groupService.GetUserSubscriptions(user.Id, 10, 10);

            // assert unsubscription works
            Assert.IsNotNull(userSubscribtionsAroundCoordintes);
            Assert.AreEqual(userSubscribtionsAroundCoordintes.Count(), 1);
            Assert.IsFalse(userSubscribtionsAroundCoordintes.Contains(groupOne.Id));
            Assert.IsTrue(userSubscribtionsAroundCoordintes.Contains(groupTwo.Id));
            Assert.IsTrue(user.HistoryGroups != null);
            Assert.IsTrue(user.HistoryGroups.Select(hgr => hgr.GroupId).Contains(groupTwo.Id));
            Assert.IsTrue(this._mockedLikkleUoW.Object.HistoryGroupRepository.AllHistoryGroups().Count() == 2);

            // arrange
            var groupThree = new Group() { Id = Guid.NewGuid(), Users = new List<User>(), IsActive = true };
            var groupFour = new Group() { Id = Guid.NewGuid(), Users = new List<User>(), IsActive = true };

            groupThree.Areas = new List<Area>() { area };
            groupFour.Areas = new List<Area>() { area };

            area.Groups.Clear();

            area.Groups.Add(groupThree);
            area.Groups.Add(groupFour);

            populatedDatabase.Groups = new FakeDbSet<Group>() { groupThree, groupFour };

            relateUserToGroupsRequest = new RelateUserToGroupsDto()
            {
                UserId = user.Id,
                Latitude = 10,
                Longitude = 10,
                GroupsUserSubscribes = new List<Guid>() { groupThree.Id, groupFour.Id }
            };

            // act
            this._subscriptionService.RelateUserToGroups(relateUserToGroupsRequest);
            userSubscribtionsAroundCoordintes = this._groupService.GetUserSubscriptions(user.Id, 10, 10);

            // assert
            Assert.IsNotNull(userSubscribtionsAroundCoordintes);
            Assert.AreEqual(userSubscribtionsAroundCoordintes.Count(), 2);
            Assert.IsFalse(userSubscribtionsAroundCoordintes.Contains(groupOne.Id));
            Assert.IsFalse(userSubscribtionsAroundCoordintes.Contains(groupTwo.Id));
            Assert.IsTrue(userSubscribtionsAroundCoordintes.Contains(groupThree.Id));
            Assert.IsTrue(userSubscribtionsAroundCoordintes.Contains(groupFour.Id));
        }

        [TestMethod]
        public void GetUserSubscriptions_Empty_List_Is_Returned_When_No_Available_Groups_For_The_User_Are_Present_When_Getting_Subscriptions()
        {
            // arrange
            var groupOne = new Group() { Id = Guid.NewGuid(), Users = new List<User>(), IsActive = true };
            var user = new User() { Id = Guid.NewGuid() };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne },
                Users = new FakeDbSet<User>() { user }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));

            // act
            var userSubscribtionsAroundCoordintes = this._groupService.GetUserSubscriptions(user.Id, 10, 10);

            // assert
            Assert.IsNotNull(userSubscribtionsAroundCoordintes);
            Assert.IsTrue(!userSubscribtionsAroundCoordintes.Any());
        }

        [TestMethod]
        public void UpdateUserLocation_When_User_Location_Is_Changed_Group_Is_Removed_But_HistoryGroup_Stays()
        {
            // arrange
            var groupOne = new Group() { Id = Guid.NewGuid(), Users = new List<User>() };
            var groupTwo = new Group() { Id = Guid.NewGuid(), Users = new List<User>() };

            var area = new Area()
            {
                Groups = new List<Group>() { groupOne, groupTwo }
            };

            groupOne.Areas = new List<Area>() { area };
            groupTwo.Areas = new List<Area>() { area };

            var user = new User() { };

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
                GroupsUserSubscribes = new List<Guid>() { groupOne.Id, groupTwo.Id }
            };

            // act
            this._subscriptionService.RelateUserToGroups(relateUserToGroupsRequest);

            Assert.IsNotNull(user.Groups);
            Assert.AreEqual(user.Groups.Count(), 2);

            Assert.IsNotNull(user.HistoryGroups);
            Assert.AreEqual(user.HistoryGroups.Count(), 2);

            this._userService.UpdateUserLocation(user.Id, 90, 90);
            Assert.AreEqual(user.Groups.Count(), 0);
            Assert.AreEqual(user.HistoryGroups.Count(), 2);
        }

        // TODO: Extract common parts from this method and the one below
        [TestMethod]
        public void RelateUserToGroups_Group_Gets_Inactive_When_No_Users_Belong_To_It()
        {
            // arrange
            this._configurationWrapperMock.Setup(config => config.AutomaticallyCleanupGroupsAndAreas).Returns(true);
            var user = new User() { };

            var groupOne = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { user }, IsActive = true };
            var groupTwo = new Group() { Id = Guid.NewGuid(), Users = new List<User>(), IsActive = true };

            var area = new Area()
            {
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne, groupTwo }
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
                Latitude = 10,
                Longitude = 10,
                GroupsUserSubscribes = new List<Guid>() { groupTwo.Id }
            };

            this._subscriptionService.RelateUserToGroups(relateUserToGroupsRequest);

            // assert
            Assert.AreEqual(2, this._areaService.GetAreaById(area.Id).Groups.Count());
            Assert.IsFalse(this._mockedLikkleUoW.Object.GroupRepository.GetGroupById(groupOne.Id).IsActive);
            Assert.IsTrue(this._mockedLikkleUoW.Object.GroupRepository.GetGroupById(groupTwo.Id).IsActive);
        }

        [TestMethod]
        public void RelateUserToGroups_Area_Gets_Inactive_When_No_Active_Groups_Belong_To_It()
        {
            // arrange
            this._configurationWrapperMock.Setup(config => config.AutomaticallyCleanupGroupsAndAreas).Returns(true);
            var user = new User() { };

            var groupOne = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { user }, IsActive = true };
            var groupTwo = new Group() { Id = Guid.NewGuid(), Users = new List<User>(), IsActive = false };

            var area = new Area()
            {
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne, groupTwo }
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
                Latitude = 10,
                Longitude = 10,
                GroupsUserSubscribes = new List<Guid>() { groupTwo.Id }
            };

            this._subscriptionService.RelateUserToGroups(relateUserToGroupsRequest);

            // assert
            Assert.AreEqual(2, this._areaService.GetAreaById(area.Id).Groups.Count());

            Assert.IsFalse(this._mockedLikkleUoW.Object.GroupRepository.GetGroupById(groupOne.Id).IsActive);
            Assert.IsFalse(this._mockedLikkleUoW.Object.GroupRepository.GetGroupById(groupTwo.Id).IsActive);

            Assert.IsFalse(this._mockedLikkleUoW.Object.AreaRepository.GetAreaById(area.Id).IsActive);
        }

        [TestMethod]
        public void AutoSubscribeUsersFromExistingAreas_We_Can_AutoSubscribe_UsersFromExistingAreas()
        {
            // arrange
            var userOne = new User()
            {
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true, AutomaticallySubscribeToAllGroupsWithTag = false
                }
            };

            var userTwo = new User()
            {
                Id = Guid.NewGuid(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = false,
                    AutomaticallySubscribeToAllGroupsWithTag = true,
                    Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList()
                }
            };

            var groupOne = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { userOne, userTwo }, IsActive = true };
            var groupTwo = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { userTwo }, IsActive = true };
            var groupThree = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { }, IsActive = true };

            userOne.Groups = new List<Group>() { groupOne };
            userTwo.Groups = new List<Group>() { groupOne, groupTwo };

            var area = new Area()
            {
                Id = Guid.NewGuid(),
                Groups = new List<Group>() { groupOne, groupTwo }
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
                new List<Guid>() { area.Id },
                new StandaloneGroupRequestDto() { TagIds = _allTags.Where(t => t.Name == "Sport").Select(t => t.Id).ToList() },
                groupThree.Id,
                Guid.NewGuid());

            // assert
            Assert.IsTrue(userOne.Groups.Select(gr => gr.Id).Contains(groupOne.Id));
            Assert.IsTrue(userOne.Groups.Select(gr => gr.Id).Contains(groupThree.Id));

            Assert.IsTrue(userTwo.Groups.Select(gr => gr.Id).Contains(groupOne.Id));
            Assert.IsTrue(userTwo.Groups.Select(gr => gr.Id).Contains(groupTwo.Id));
            Assert.IsTrue(userTwo.Groups.Select(gr => gr.Id).Contains(groupThree.Id));
        }

        [TestMethod]
        public void AutoSubscribeUsersForGroupAsNewArea_We_Can_AutoSubscribe_UsersForGroupAsNewArea()
        {
            // arrange
            var userOne = new User()
            {
                Id = Guid.NewGuid(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Latitude = 10.000001,
                Longitude = 10.000001,
                Groups = new List<Group>() { }
            };

            var userTwo = new User()
            {
                Id = Guid.NewGuid(),
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

            var userThree = new User()
            {
                Id = Guid.NewGuid(),
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

            var groupThree = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Users = new FakeDbSet<User>() { userOne, userTwo, userThree },
                Groups = new FakeDbSet<Group>() { groupThree }
            }
            .Seed();
            DataGenerator.SetupUserAndGroupRepositories(this._mockedLikkleUoW, populatedDatabase);

            // act
            this._subscriptionService.AutoSubscribeUsersForGroupAsNewArea(
                Guid.NewGuid(),
                10.000000,
                10.000000,
                BusinessEntities.Enums.RadiusRangeEnum.FiftyMeters,
                groupThree.Id,
                Guid.NewGuid());

            // assert
            Assert.IsTrue(userOne.Groups.Contains(groupThree));
            Assert.IsTrue(userTwo.Groups.Contains(groupThree));
            Assert.IsFalse(userThree.Groups.Contains(groupThree));
        }

        [TestMethod]
        public void AutoSubscribeUsersForRecreatedGroup_We_Can_AutoSubscribe_UsersForGroupRecreated_And_Ping_Correct_Set_Of_Users_Via_SignalR()
        {
            // arrange
            var userOne = new User()
            {
                Id = Guid.NewGuid(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = false,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Latitude = 10.000001,
                Longitude = 10.000001,
                Groups = new List<Group>() { }
            };

            var userTwo = new User()
            {
                Id = Guid.NewGuid(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = false,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Latitude = 10.000002,
                Longitude = 10.000002,
                Groups = new List<Group>() { }
            };

            var userThree = new User()
            {
                Id = Guid.NewGuid(),
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

            var userFour = new User()
            {
                Id = Guid.NewGuid(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Latitude = 10.000004,
                Longitude = 10.000004,
                Groups = new List<Group>() { }
            };

            var group = new Group()
            {
                Id = Guid.NewGuid(),
                Users = new List<User>() { },
                IsActive = true,
                Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList()
            };

            var area = new Area()
            {
                Id = Guid.NewGuid(),
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
            this._subscriptionService.AutoSubscribeUsersForRecreatedGroup(new List<Guid>() { area.Id }, group.Id, userOne.Id);

            // assert
            _signalrServiceMock.Verify(m => m.GroupAroundMeWasRecreated(
                It.IsIn<string>(new string[] { userTwo.Id.ToString(), userThree.Id.ToString(), userFour.Id.ToString() }),
                It.Is<List<SRAreaDto>>(a => a.Count() == 1),
                It.Is<SRGroupDto>(g => g.Id == group.Id),
                It.IsAny<bool>()),
                Times.Exactly(3));
        }

        [TestMethod]
        public void AutoSubscribeUsersForGroupAsNewArea_We_Can_AutoSubscribe_UsersForGroupAsNewArea_And_Ping_Correct_Set_Of_Users_Via_SignalR()
        {
            // arrange
            var userOne = new User()
            {
                Id = Guid.NewGuid(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = false,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Latitude = 10.000001,
                Longitude = 10.000001,
                Groups = new List<Group>() { }
            };

            var userTwo = new User()
            {
                Id = Guid.NewGuid(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = false,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Latitude = 10.000002,
                Longitude = 10.000002,
                Groups = new List<Group>() { }
            };

            var userThree = new User()
            {
                Id = Guid.NewGuid(),
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

            var userFour = new User()
            {
                Id = Guid.NewGuid(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Latitude = 10.000004,
                Longitude = 10.000004,
                Groups = new List<Group>() { }
            };

            var group = new Group() {
                Id = Guid.NewGuid(),
                Users = new List<User>() { },
                IsActive = true,
                Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList()
            };

            var area = new Area()
            {
                Id = Guid.NewGuid(),
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
                group.Id,
                userOne.Id);

            // assert
            _signalrServiceMock.Verify(m => m.GroupAsNewAreaWasCreatedAroundMe(
                It.IsIn<string>(new string[] { userTwo.Id.ToString(), userThree.Id.ToString(), userFour.Id.ToString() }),
                It.IsAny<SRAreaDto>(),
                It.IsAny<SRGroupDto>(),
                It.IsAny<bool>()),
                Times.Exactly(3));
        }

        [TestMethod]
        public void AutoSubscribeUsersForRecreatedGroup_We_Can_AutoSubscribe_UsersForRecreatedGroup()
        {
            // arrange
            var userOne = new User()
            {
                Id = Guid.NewGuid(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Latitude = 10.000001,
                Longitude = 10.000001,
                Groups = new List<Group>() { }
            };

            var groupOne = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { }, IsActive = false, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };

            var area = new Area()
            {
                Id = Guid.NewGuid(),
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
            this._subscriptionService.AutoSubscribeUsersForRecreatedGroup(new List<Guid>() { area.Id }, groupOne.Id, Guid.NewGuid());

            // assert
            Assert.IsTrue(userOne.Groups.Contains(groupOne));
        }

        [TestMethod]
        public void AutoSubscribeUsersFromExistingAreas_When_AutoSubscribe_UsersFromExistingAreas_User_Who_Fired_The_Action_Does_Not_Get_Notified()
        {
            // arrange
            var userOne = new User()
            {
                Id = Guid.NewGuid(),
                Groups = new List<Group>() { }
            };

            var userTwo = new User()
            {
                Id = Guid.NewGuid(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Groups = new List<Group>() { }
            };

            var userThree = new User()
            {
                Id = Guid.NewGuid(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                },
                Groups = new List<Group>() { }
            };

            var groupOne = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { userOne, userTwo, userThree }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };
            var groupTwo = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };

            userOne.Groups.Add(groupOne);
            userTwo.Groups.Add(groupOne);
            userThree.Groups.Add(groupOne);

            var area = new Area()
            {
                Id = Guid.NewGuid(),
                Groups = new List<Group>() { groupOne }
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
            this._subscriptionService.AutoSubscribeUsersFromExistingAreas(new List<Guid>() { area.Id }, new StandaloneGroupRequestDto() { }, groupTwo.Id, userOne.Id);

            // assert
            this._signalrServiceMock.Verify(srs => srs.GroupAttachedToExistingAreasWasCreatedAroundMe(It.IsAny<string>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<SRGroupDto>(), It.IsAny<bool>()), Times.Exactly(2));
        }

        [TestMethod]
        public void AutoSubscribeUsersForGroupAsNewArea_When_AutoSubscribe_UsersForGroupAsNewArea_User_Who_Fired_The_Action_Does_Not_Get_Notified()
        {
            // arrange
            var userOne = new User() { Id = Guid.NewGuid() };

            var userTwo = new User()
            {
                Id = Guid.NewGuid(),
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

            var groupOne = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { userOne }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };

            var area = new Area()
            {
                Id = Guid.NewGuid(),
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
            this._subscriptionService.AutoSubscribeUsersForGroupAsNewArea(area.Id, area.Latitude, area.Longitude, area.Radius, groupOne.Id, userOne.Id);

            // assert
            this._signalrServiceMock.Verify(srs => srs.GroupAsNewAreaWasCreatedAroundMe(It.IsAny<string>(), It.IsAny<SRAreaDto>(), It.IsAny<SRGroupDto>(), It.IsAny<bool>()), Times.Once);
        }

        [TestMethod]
        public void AutoSubscribeUsersForRecreatedGroup_When_AutoSubscribe_UsersForRecreatedGroup_User_Who_Fired_The_Action_Does_Not_Get_Notified()
        {
            // arrange
            var groupOne = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };
            var areaOne = new Area() { Id = Guid.NewGuid() };

            var areaTwo = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = 10,
                Longitude = 10,
                Radius = BusinessEntities.Enums.RadiusRangeEnum.HunderdAndFiftyMeters
            };

            groupOne.Areas = new List<Area>() { areaOne, areaTwo };

            var userOne = new User()
            {
                Id = Guid.NewGuid(),
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
            this._subscriptionService.AutoSubscribeUsersForRecreatedGroup(new List<Guid>() { areaOne.Id, areaTwo.Id }, groupOne.Id, Guid.NewGuid());

            // assert
            this._signalrServiceMock.Verify(
                srs => srs.GroupAroundMeWasRecreated(It.IsAny<string>(),
                It.IsAny<IEnumerable<SRAreaDto>>(),
                It.IsAny<SRGroupDto>(),
                It.IsAny<bool>()),
                Times.Once);
        }

        [TestMethod]
        public void AutoIncreaseUsersInGroups_When_AutoIncreaseUsersInGroups_Is_Called_User_Who_Fired_The_Action_Does_Not_Get_Notified()
        {
            // arrange
            var groupOne = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };
            var groupTwo = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };
            var groupThree = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };

            var areaOne = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne, groupTwo, groupThree },
                Radius = BusinessEntities.Enums.RadiusRangeEnum.FiftyMeters
            };

            groupOne.Areas = new List<Area>() { areaOne };
            groupTwo.Areas = new List<Area>() { areaOne };

            var userOne = new User()
            {
                Id = Guid.NewGuid(),
                HistoryGroups = new List<HistoryGroup>()
            };
            userOne.HistoryGroups.Add(new HistoryGroup { GroupId = groupOne.Id, UserId = userOne.Id, UserWhoSubscribedGroup = userOne });
            userOne.HistoryGroups.Add(new HistoryGroup { GroupId = groupTwo.Id, UserId = userOne.Id, UserWhoSubscribedGroup = userOne });

            groupOne.Users.Add(userOne);
            groupTwo.Users.Add(userOne);

            var userTwo = new User()
            {
                Id = Guid.NewGuid(),
                Latitude = 10.000001,
                Longitude = 10.000001,
                HistoryGroups = new List<HistoryGroup>()
            };
            userOne.HistoryGroups.Add(new HistoryGroup { GroupId = groupOne.Id, UserId = userTwo.Id, UserWhoSubscribedGroup = userTwo });
            groupOne.Users.Add(userTwo);

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo, groupThree },
                Users = new FakeDbSet<User>() { userOne, userTwo },
                Areas = new FakeDbSet<Area>() { areaOne }
            }
            .Seed();
            DataGenerator.SetupAreaUserAndGroupRepositories(this._mockedLikkleUoW, populatedDatabase);

            // act
            this._subscriptionService.AutoIncreaseUsersInGroups(new List<Guid>() { groupThree.Id }, userOne.Id);

            // assert
            this._signalrServiceMock.Verify(srs => srs.GroupWasJoinedByUser(groupThree.Id, It.IsAny<List<string>>()), Times.Once);
        }

        [TestMethod]
        public void AutoDecreaseUsersInGroups_When_AutoDecreaseUsersInGroups_Is_Called_User_Who_Fired_The_Action_Does_Not_Get_Notified()
        {
            // arrange
            var groupOne = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };
            var groupTwo = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };

            var areaOne = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne, groupTwo },
                IsActive = true,
                Radius = BusinessEntities.Enums.RadiusRangeEnum.FiftyMeters
            };

            groupOne.Areas = new List<Area>() { areaOne };
            groupTwo.Areas = new List<Area>() { areaOne };

            var userOne = new User()
            {
                Id = Guid.NewGuid(),
                HistoryGroups = new List<HistoryGroup>()
            };

            userOne.HistoryGroups.Add(new HistoryGroup() { GroupId = groupOne.Id, UserId = userOne.Id, UserWhoSubscribedGroup = userOne });
            userOne.HistoryGroups.Add(new HistoryGroup() { GroupId = groupTwo.Id, UserId = userOne.Id, UserWhoSubscribedGroup = userOne });
            groupOne.Users.Add(userOne);
            groupTwo.Users.Add(userOne);

            var userTwo = new User()
            {
                Id = Guid.NewGuid(),
                Latitude = 10.000001,
                Longitude = 10.000001,
                HistoryGroups = new List<HistoryGroup>()
            };
            userOne.HistoryGroups.Add(new HistoryGroup() { GroupId = groupOne.Id, UserId = userTwo.Id, UserWhoSubscribedGroup = userTwo });
            groupOne.Users.Add(userTwo);

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo },
                Users = new FakeDbSet<User>() { userOne, userTwo },
                Areas = new FakeDbSet<Area>() { areaOne }
            }
            .Seed();
            DataGenerator.SetupAreaUserAndGroupRepositories(this._mockedLikkleUoW, populatedDatabase);

            // act
            this._subscriptionService.AutoDecreaseUsersInGroups(new List<Guid>() { groupTwo.Id }, userOne.Id);

            // assert
            this._signalrServiceMock.Verify(srs => srs.GroupWasLeftByUser(groupTwo.Id, It.IsAny<List<string>>()), Times.Once);
        }

        [TestMethod]
        public void AutoDecreaseUsersInGroups_When_AutoDecreaseUsersInGroup_Is_Called_HistoryGroups_Are_AlsoConsidered()
        {
            // arrange
            var groupOne = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };
            var groupTwo = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };

            var userOne = new User()
            {
                Id = Guid.NewGuid(),
                HistoryGroups = new List<HistoryGroup>() { }
            };
            userOne.HistoryGroups.Add(new HistoryGroup() { GroupId = groupOne.Id, UserId = userOne.Id, UserWhoSubscribedGroup = userOne });
            userOne.HistoryGroups.Add(new HistoryGroup() { GroupId = groupTwo.Id, UserId = userOne.Id, UserWhoSubscribedGroup = userOne });
            groupOne.Users.Add(userOne);
            groupTwo.Users.Add(userOne);

            var userTwo = new User()
            {
                Id = Guid.NewGuid(),
                Latitude = 10.000001,
                Longitude = 10.000001,
                HistoryGroups = new List<HistoryGroup>() { }
            };

            var areaOne = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne, groupTwo },
                IsActive = true,
                Radius = BusinessEntities.Enums.RadiusRangeEnum.FiftyMeters
            };

            groupOne.Areas = new List<Area>() { areaOne };
            groupTwo.Areas = new List<Area>() { areaOne };

            var historyGroupOneId = Guid.NewGuid();
            var historyGroupOne = new HistoryGroup()
            {
                GroupId = groupOne.Id,
                UserId = userOne.Id,
                Id = historyGroupOneId,
                GroupThatWasPreviouslySubscribed = groupOne,
                UserWhoSubscribedGroup = userOne
            };
            userOne.HistoryGroups.Add(new HistoryGroup() { GroupId = groupOne.Id, UserId = userOne.Id, UserWhoSubscribedGroup = userOne });

            var historyGroupTwoId = Guid.NewGuid();
            var historyGroupTwo = new HistoryGroup()
            {
                GroupId = groupTwo.Id,
                UserId = userOne.Id,
                Id = historyGroupTwoId,
                GroupThatWasPreviouslySubscribed = groupTwo,
                UserWhoSubscribedGroup = userOne
            };
            userOne.HistoryGroups.Add(new HistoryGroup() { GroupId = groupTwo.Id, UserId = userOne.Id, UserWhoSubscribedGroup = userOne });

            var historyGroupThree = new HistoryGroup()
            {
                GroupId = groupTwo.Id,
                UserId = userTwo.Id,
                Id = Guid.NewGuid(),
                GroupThatWasPreviouslySubscribed = groupTwo,
                UserWhoSubscribedGroup = userTwo
            };

            userTwo.HistoryGroups.Add(new HistoryGroup() { GroupId = groupTwo.Id, UserId = userTwo.Id, UserWhoSubscribedGroup = userTwo });

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo },
                Users = new FakeDbSet<User>() { userOne, userTwo },
                Areas = new FakeDbSet<Area>() { areaOne },
                HistoryGroups = new FakeDbSet<HistoryGroup> { historyGroupOne, historyGroupTwo, historyGroupThree }
            }
            .Seed();
            DataGenerator.SetupMockedRepositories(this._mockedLikkleUoW, populatedDatabase);

            // act
            this._subscriptionService.AutoDecreaseUsersInGroups(new List<Guid>() { groupTwo.Id }, userOne.Id);

            // assert
            this._signalrServiceMock.Verify(srs => srs.GroupWasLeftByUser(groupTwo.Id, new List<string>() { userTwo.Id.ToString() }), Times.Once);
        }

        [TestMethod]
        public void AutoDecreaseUsersInGroups_When_SignalR_Has_To_Ping_People_For_UserLeftOrJoinedArea_If_User_Outside_Of_Area_Range_Doesnt_Get_Notified()
        {
            // arrange
            var groupOne = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };

            var userOne = new User()
            {
                Id = Guid.NewGuid(),
                HistoryGroups = new List<HistoryGroup>() { }
            };
            userOne.HistoryGroups.Add(new HistoryGroup() { GroupId = groupOne.Id, UserId = userOne.Id, UserWhoSubscribedGroup = userOne });

            var userTwo = new User()
            {
                HistoryGroups = new List<HistoryGroup>() { },
                Latitude = 10.000001,
                Longitude = 10.000001
            };
            userTwo.HistoryGroups.Add(new HistoryGroup() { GroupId = groupOne.Id, UserId = userTwo.Id, UserWhoSubscribedGroup = userTwo });

            var userThree = new User()
            {
                HistoryGroups = new List<HistoryGroup>(),
                Latitude = 42.123456,
                Longitude = 21.123456
            };
            userThree.HistoryGroups.Add(new HistoryGroup() { GroupId = groupOne.Id, UserId = userThree.Id, UserWhoSubscribedGroup = userThree });

            var areaOne = new Area()
            {
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne },
                IsActive = true,
                Radius = BusinessEntities.Enums.RadiusRangeEnum.FiftyMeters
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne },
                Users = new FakeDbSet<User>() { userOne, userTwo, userThree },
                Areas = new FakeDbSet<Area>() { areaOne }
            }
           .Seed();
            DataGenerator.SetupMockedRepositories(this._mockedLikkleUoW, populatedDatabase);

            // act
            this._subscriptionService.AutoDecreaseUsersInGroups(new List<Guid>() { groupOne.Id }, userOne.Id);

            // assert
            this._signalrServiceMock.Verify(srs => srs.GroupWasLeftByUser(groupOne.Id, new List<string>() { userTwo.Id.ToString() }), Times.Once);
        }

        [TestMethod]
        public void AutoIncreaseUsersInGroups_Users_Who_Came_For_The_First_Time_In_The_Area_And_Never_Subscribed_Groups_There_Also_Gets_UserLeftJoined_Events()
        {
            // arrange
            var groupOne = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };
            var userOne = new User()
            {
                Id = Guid.NewGuid(),
                HistoryGroups = new List<HistoryGroup>()
            };

            userOne.HistoryGroups.Add(new HistoryGroup { GroupId = groupOne.Id, UserId = userOne.Id, UserWhoSubscribedGroup = userOne });

            var userTwo = new User()
            {
                Id = Guid.NewGuid(),
                HistoryGroups = new List<HistoryGroup>(),
                Latitude = 10.000002,
                Longitude = 10.000002
            };

            var userThree = new User()
            {
                Id = Guid.NewGuid(),
                HistoryGroups = new List<HistoryGroup>()
            };
            userOne.HistoryGroups.Add(new HistoryGroup { GroupId = groupOne.Id, UserId = userThree.Id, UserWhoSubscribedGroup = userThree });

            var areaOne = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne },
                Radius = BusinessEntities.Enums.RadiusRangeEnum.FiftyMeters
            };
            groupOne.Areas = new List<Area>() { areaOne };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne },
                Users = new FakeDbSet<User>() { userOne, userTwo, userThree },
                Areas = new FakeDbSet<Area>() { areaOne }
            }
            .Seed();
            DataGenerator.SetupAreaUserAndGroupRepositories(this._mockedLikkleUoW, populatedDatabase);

            // act
            this._subscriptionService.AutoIncreaseUsersInGroups(new List<Guid>() { groupOne.Id }, userOne.Id);

            // assert
            this._signalrServiceMock.Verify(srs => srs.GroupWasJoinedByUser(groupOne.Id, new List<string>() { userTwo.Id.ToString() }), Times.Once);
        }

        [TestMethod]
        public void AutoSubscribeUsersFromExistingAreas_Users_Falling_Under_The_Area_Not_Part_Of_Active_Groups_Are_Considered()
        {
            // arrange
            var userOne = new User() {
                Id = Guid.NewGuid(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting() {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false,
                    Tags = null
                },
                Groups = new List<Group>()
            };
            var group = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { userOne }, IsActive = true, Tags = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList() };

            var area = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { group },
                Radius = BusinessEntities.Enums.RadiusRangeEnum.FiftyMeters
            };

            var userTwo = new User() {
                Id = Guid.NewGuid(),
                Latitude = 10.000001,
                Longitude = 10.000001,
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false,
                    Tags = null
                },
                Groups = new List<Group>()
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { group },
                Users = new FakeDbSet<User>() { userOne, userTwo },
                Areas = new FakeDbSet<Area>() { area }
            }
            .Seed();
            DataGenerator.SetupAreaUserAndGroupRepositories(this._mockedLikkleUoW, populatedDatabase);

            // act
            _subscriptionService.AutoSubscribeUsersFromExistingAreas(
                new List<Guid>() { area.Id }, new StandaloneGroupRequestDto(), Guid.NewGuid(), Guid.NewGuid());

            // assert
            _signalrServiceMock.Verify(
                srsm => srsm.GroupAttachedToExistingAreasWasCreatedAroundMe(It.Is<string>(uid => uid == userTwo.Id.ToString()), It.IsAny<IEnumerable<Guid>>(), It.IsAny<SRGroupDto>(), It.IsAny<bool>()), Times.Once);
        }

        [TestMethod]
        [DataRow(false, false, 1)]
        [DataRow(true, false, 1)]
        [DataRow(false, true, 1)]
        public void AutoSubscribeUsersFromExistingAreas_SubscribeUsersNearbyNewGroupBasedOnTheirAutomaticSubscrSetttings(
            bool userSubscribesAllAvailableGroups,
            bool userSubscribesAllGroupsWithTag,
            int numberOfCallsToSignalR)
        {
            // arrange
            var tagsSample = _allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList();

            var user = new User()
            {
                Id = Guid.NewGuid(),
                AutomaticSubscriptionSettings = new AutomaticSubscriptionSetting()
                {
                    AutomaticallySubscribeToAllGroups = userSubscribesAllAvailableGroups,
                    AutomaticallySubscribeToAllGroupsWithTag = userSubscribesAllGroupsWithTag,
                    Tags = userSubscribesAllGroupsWithTag ? tagsSample : null
                },
                Groups = new List<Group>(),
                Latitude = 10.000001,
                Longitude = 10.000001
            };

            var area = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { },
                Radius = BusinessEntities.Enums.RadiusRangeEnum.FiftyMeters
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Users = new FakeDbSet<User>() { user },
                Areas = new FakeDbSet<Area>() { area }
            }
            .Seed();
            DataGenerator.SetupAreaUserAndGroupRepositories(this._mockedLikkleUoW, populatedDatabase);

            // act
            _subscriptionService.AutoSubscribeUsersFromExistingAreas(
                new List<Guid>() { area.Id }, new StandaloneGroupRequestDto() { TagIds = tagsSample.Select(t => t.Id) }, Guid.NewGuid(), Guid.NewGuid());

            // assert
            _signalrServiceMock.Verify(
                srsm => srsm
                .GroupAttachedToExistingAreasWasCreatedAroundMe(
                    It.Is<string>(uid => uid == user.Id.ToString()), 
                    It.IsAny<IEnumerable<Guid>>(), 
                    It.IsAny<SRGroupDto>(), 
                    It.IsAny<bool>()), Times.Exactly(numberOfCallsToSignalR));
        }
    }
}
