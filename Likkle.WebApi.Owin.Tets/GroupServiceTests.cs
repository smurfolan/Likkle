using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
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
    public class GroupServiceTests
    {
        private readonly IGroupService _groupService;

        private readonly Mock<ILikkleUoW> _mockedLikkleUoW;
        private readonly Mock<IConfigurationProvider> _mockedConfigurationProvider;
        private readonly Mock<IConfigurationWrapper> _configurationWrapperMock;

        public GroupServiceTests()
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

            this._groupService = new GroupService(
                this._mockedLikkleUoW.Object,
                this._mockedConfigurationProvider.Object,
                this._configurationWrapperMock.Object);
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
            var newGroupId = this._groupService.InsertNewGroup(newGroupRequest);

            // assert
            Assert.IsTrue(newGroupId != Guid.Empty);

            var newlyCreatedGroup = this._groupService.GetGroupById(newGroupId);

            Assert.IsNotNull(newlyCreatedGroup);
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
            var allUsersInGroup = this._groupService.GetUsersFromGroup(groupId);

            // assert
            Assert.IsNotNull(allUsersInGroup);
            Assert.AreEqual(allUsersInGroup.Count(), 2);
        }

        [TestMethod]
        public void StandaloneGroup_We_Create_Gets_Into_Our_HistoryGroups()
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
            var newGroupId = this._groupService.InsertNewGroup(newGroupRequest);

            // assert
            var user = this._mockedLikkleUoW.Object.UserRepository.GetUserById(userId);
            Assert.IsNotNull(user.HistoryGroups);
            Assert.IsTrue(user.HistoryGroups.Select(hg => hg.GroupId).Contains(newGroupId));
        }

        [TestMethod]
        public void GroupAsNewArea_We_Create_Gets_Into_Our_HistoryGroups()
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

            var newGroupRequest = new GroupAsNewAreaRequestDto()
            {
                Name = "New group",
                TagIds = new List<Guid>() { Guid.Parse("caf77dee-a94f-49cb-b51f-e0c0e1067541"), Guid.Parse("bd456f08-f137-4382-8358-d52772c2dfc8") },
                Radius = RadiusRangeEnum.FiveHundredMeters,
                UserId = userId,
                Latitude = 10,
                Longitude = 10
            };

            // act 
            var newGroupId = this._groupService.InserGroupAsNewArea(newGroupRequest);

            // assert
            var user = this._mockedLikkleUoW.Object.UserRepository.GetUserById(userId);
            Assert.IsNotNull(user.HistoryGroups);
            Assert.IsTrue(user.HistoryGroups.Select(hg => hg.GroupId).Contains(newGroupId));
        }
    }
}
