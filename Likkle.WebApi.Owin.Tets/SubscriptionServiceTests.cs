using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Likkle.BusinessEntities;
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
    public class SubscriptionServiceTests
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IGroupService _groupService;
        private readonly IUserService _userService;
        private readonly IAreaService _areaService;

        private readonly Mock<ILikkleUoW> _mockedLikkleUoW;
        private readonly Mock<IConfigurationProvider> _mockedConfigurationProvider;
        private readonly Mock<IConfigurationWrapper> _configurationWrapperMock;

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

            var mapConfiguration = new MapperConfiguration(cfg => {
                cfg.AddProfile<EntitiesMappingProfile>();
            });
            this._mockedConfigurationProvider.Setup(mc => mc.CreateMapper()).Returns(mapConfiguration.CreateMapper);

            this._configurationWrapperMock = new Mock<IConfigurationWrapper>();

            this._subscriptionService = new SubscriptionService(
                this._mockedLikkleUoW.Object,
                this._mockedConfigurationProvider.Object,
                this._configurationWrapperMock.Object);

            this._groupService = new GroupService(
                this._mockedLikkleUoW.Object,
                this._mockedConfigurationProvider.Object,
                this._configurationWrapperMock.Object);

            this._userService = new UserService(
                this._mockedLikkleUoW.Object,
                this._mockedConfigurationProvider.Object,
                this._configurationWrapperMock.Object);

            this._areaService = new AreaService(
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

            // act
            this._subscriptionService.RelateUserToGroups(relateUserToGroupsRequest);
            userSubscribtionsAroundCoordintes = this._groupService.GetUserSubscriptions(userId, 10, 10);

            // assert unsubscription works
            Assert.IsNotNull(userSubscribtionsAroundCoordintes);
            Assert.AreEqual(userSubscribtionsAroundCoordintes.Count(), 1);
            Assert.IsFalse(userSubscribtionsAroundCoordintes.Contains(groupOneId));
            Assert.IsTrue(userSubscribtionsAroundCoordintes.Contains(groupTwoId));
            Assert.IsTrue(user.HistoryGroups != null);
            Assert.AreEqual(user.HistoryGroups.Count(), 1);
            Assert.IsTrue(user.HistoryGroups.Select(hgr => hgr.GroupId).Contains(groupTwoId));


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
            this._subscriptionService.RelateUserToGroups(relateUserToGroupsRequest);

            Assert.AreEqual(2, this._groupService.AllGroups().Count());
            groupOne.Users.Add(user);
            groupTwo.Users.Add(user);
            Assert.IsTrue(this._groupService.AllGroups().Select(a => a.Id).Contains(groupOneId));
            Assert.IsTrue(this._groupService.AllGroups().Select(a => a.Id).Contains(groupTwoId));

            this._mockedLikkleUoW.Setup(uow => uow.Save()).Callback((() =>
            {
                area.Groups = new List<Group>();
            }));

            this._subscriptionService.RelateUserToGroups(relateUserToGroupsRequestWithUnsubscribedGroups);

            Assert.AreEqual(0, this._groupService.AllGroups().Count());
            Assert.IsTrue(!this._areaService.GetAllAreas().Any());
            Assert.IsFalse(this._groupService.AllGroups().Select(a => a.Id).Contains(groupOneId));
            Assert.IsFalse(this._groupService.AllGroups().Select(a => a.Id).Contains(groupTwoId));
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
            this._subscriptionService.RelateUserToGroups(relateUserToGroupsRequest);

            Assert.AreEqual(4, this._groupService.AllGroups().Count());
            groupOne.Users.Add(user);
            groupTwo.Users.Add(user);
            Assert.IsTrue(this._groupService.AllGroups().Select(a => a.Id).Contains(groupOneId));
            Assert.IsTrue(this._groupService.AllGroups().Select(a => a.Id).Contains(groupTwoId));

            this._subscriptionService.RelateUserToGroups(relateUserToGroupsRequestWithUnsubscribedGroups);

            Assert.AreEqual(2, this._groupService.AllGroups().Count());
            Assert.IsTrue(this._areaService.GetAllAreas().Any());
            Assert.IsFalse(this._groupService.AllGroups().Select(a => a.Id).Contains(groupOneId));
            Assert.IsFalse(this._groupService.AllGroups().Select(a => a.Id).Contains(groupTwoId));
        }
    }
}
