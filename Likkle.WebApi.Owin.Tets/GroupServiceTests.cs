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

        [TestMethod]
        public void We_Can_Get_List_Of_Previously_Created_Groups_When_At_A_Place_With_Previous_Activity()
        {
            // arrange
            var userId = Guid.NewGuid();
            var dbUser = new User()
            {
                Id = userId,
                NotificationSettings = null
            };

            var groupOneId = Guid.NewGuid();
            var groupOne = new Group()
            {
                Id = groupOneId,
                Name = "Group one",
                IsActive = false
            };

            var groupTwoId = Guid.NewGuid();
            var groupTwo = new Group()
            {
                Id = groupTwoId,
                Name = "Group two",
                IsActive = true
            };

            var groupThreeId = Guid.NewGuid();
            var groupThree = new Group()
            {
                Id = groupThreeId,
                Name = "Group three",
                IsActive = false
            };

            dbUser.HistoryGroups = new List<HistoryGroup>() {
                new HistoryGroup()
                {
                    DateTimeGroupWasSubscribed = DateTime.UtcNow.AddDays(-2),
                    GroupId = groupOneId,
                    GroupThatWasPreviouslySubscribed = groupOne,
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    UserWhoSubscribedGroup = dbUser
                },
                new HistoryGroup()
                {
                    DateTimeGroupWasSubscribed = DateTime.UtcNow.AddDays(-2),
                    GroupId = groupThreeId,
                    GroupThatWasPreviouslySubscribed = groupThree,
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    UserWhoSubscribedGroup = dbUser
                }
            };

            groupOne.Users = new List<User>() { dbUser };
            groupTwo.Users = new List<User>() { dbUser };
            groupThree.Users = new List<User>() { dbUser };

            var areaOneId = Guid.NewGuid();
            var areaOne = new Area()
            {
                Id = areaOneId,
                Latitude = 10.0000000,
                Longitude = 10.0000000,
                Radius = RadiusRangeEnum.FiftyMeters,
                IsActive = true,
                Groups = new List<Group>() { groupOne, groupTwo }
            };

            var areaTwoId = Guid.NewGuid();
            var areaTwo = new Area()
            {
                Id = areaTwoId,
                Latitude = 10.0000001,
                Longitude = 10.0000001,
                Radius = RadiusRangeEnum.HunderdAndFiftyMeters,
                IsActive = true,
                Groups = new List<Group>() { groupOne, groupTwo, groupThree }
            };

            groupOne.Areas = new List<Area>() { areaOne, areaTwo };
            groupTwo.Areas = new List<Area>() { areaOne, areaTwo };
            groupThree.Areas = new List<Area>() { areaTwo };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Users = new FakeDbSet<User>() { dbUser },
                Areas = new FakeDbSet<Area>() { areaOne, areaTwo },
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo, groupThree }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.TagRepository).Returns(new TagRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));

            // act
            var result = this._groupService.GetGroupCreationType(10, 10, userId);

            // assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.PrevousGroupsList);
            Assert.AreEqual(result.CreationType, CreateGroupActionTypeEnum.ListOfPreviouslyCreatedOrSubscribedGroups);
            Assert.AreEqual(2, result.PrevousGroupsList.Count());
        }

        [TestMethod]
        public void We_Get_AutomaticallyGroupAsNewArea_Response_When_Never_Created_Or_Subscribed_Group_Here()
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
                Radius = RadiusRangeEnum.FiftyMeters,
                IsActive = true
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

            // act
            var result = this._groupService.GetGroupCreationType(20, 20, userId);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.CreationType, CreateGroupActionTypeEnum.AutomaticallyGroupAsNewArea);
        }

        [TestMethod]
        public void We_Get_Choice_Screen_When_Creating_A_group_In_Existing_Active_Area()
        {
            // arrange
            var userId = Guid.NewGuid();
            var dbUser = new User()
            {
                Id = userId,
                NotificationSettings = null
            };

            var groupOneId = Guid.NewGuid();
            var groupOne = new Group()
            {
                Id = groupOneId,
                IsActive = true
            };

            var groupTwoId = Guid.NewGuid();
            var groupTwo = new Group()
            {
                Id = groupTwoId,
                IsActive = true
            };

            var newAreaId = Guid.NewGuid();
            var newArea = new Area()
            {
                Id = newAreaId,
                Latitude = 10,
                Longitude = 10,
                Radius = RadiusRangeEnum.FiftyMeters,
                IsActive = true,
                Groups = new List<Group>() { groupOne, groupTwo }
            };

            groupOne.Areas = new List<Area>() { newArea };
            groupTwo.Areas = new List<Area>() { newArea };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Users = new FakeDbSet<User>() { dbUser },
                Areas = new FakeDbSet<Area>() { newArea },
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.TagRepository).Returns(new TagRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));

            // act
            var result = this._groupService.GetGroupCreationType(10, 10, userId);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.CreationType, CreateGroupActionTypeEnum.ChoiceScreen);
        }

        [TestMethod]
        public void We_Can_Activate_Previously_Not_Active_Group()
        {
            // arrange

            var groupOneId = Guid.NewGuid();
            var groupOne = new Group()
            {
                Id = groupOneId,
                IsActive = true
            };

            var newAreaId = Guid.NewGuid();
            var newArea = new Area()
            {
                Id = newAreaId,
                Latitude = 10,
                Longitude = 10,
                Radius = RadiusRangeEnum.FiftyMeters,
                IsActive = false,
                Groups = new List<Group>() { groupOne }
            };

            var secondNewAreaId = Guid.NewGuid();
            var secondNewArea = new Area()
            {
                Id = secondNewAreaId,
                Latitude = 10,
                Longitude = 10,
                Radius = RadiusRangeEnum.FiveHundredMeters,
                IsActive = false,
                Groups = new List<Group>() {groupOne}
            };

            groupOne.Areas = new List<Area>() { newArea, secondNewArea };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Areas = new FakeDbSet<Area>() { newArea, secondNewArea },
                Groups = new FakeDbSet<Group>() { groupOne }
            }
            .Seed();
            
            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));

            // act
            this._groupService.ActivateGroup(groupOneId);

            // assert
            Assert.IsTrue(groupOne.IsActive);
            Assert.IsTrue(newArea.IsActive);
            Assert.IsTrue(secondNewArea.IsActive);
        }
    }
}
