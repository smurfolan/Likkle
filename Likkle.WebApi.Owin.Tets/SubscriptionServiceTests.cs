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

            this._configurationWrapperMock = new Mock<IConfigurationWrapper>();

            this._subscriptionService = new SubscriptionService(
                this._mockedLikkleUoW.Object,
                this._mockedConfigurationProvider.Object,
                this._configurationWrapperMock.Object);

            this._groupService = new GroupService(
                this._mockedLikkleUoW.Object,
                this._mockedConfigurationProvider.Object,
                this._configurationWrapperMock.Object,
                this._geoCodingManagerMock.Object);

            this._userService = new UserService(
                this._mockedLikkleUoW.Object,
                this._mockedConfigurationProvider.Object,
                this._configurationWrapperMock.Object,
                this._accelometerAlgorithmHelperService.Object,
                this._subscriptionSettingsService.Object);

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
    }
}
