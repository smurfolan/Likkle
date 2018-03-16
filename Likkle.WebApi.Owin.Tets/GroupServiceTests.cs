using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Likkle.BusinessEntities.Enums;
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
    public class GroupServiceTests
    {
        private readonly IGroupService _groupService;

        private readonly Mock<ILikkleUoW> _mockedLikkleUoW;
        private readonly Mock<IConfigurationProvider> _mockedConfigurationProvider;
        private readonly Mock<IConfigurationWrapper> _configurationWrapperMock;
        private readonly Mock<IGeoCodingManager> _geoCodingManagerMock;
        private readonly Mock<ISubscriptionService> _subscrServiceMock;

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

            _geoCodingManagerMock = new Mock<IGeoCodingManager>();
            _geoCodingManagerMock.Setup(gcm => gcm.GetApproximateAddress(It.IsAny<NewAreaRequest>()))
                .Returns(Guid.NewGuid().ToString);

            _subscrServiceMock = new Mock<ISubscriptionService>();
            _subscrServiceMock.Setup(ssm => ssm.AutoSubscribeUsersFromExistingAreas(It.IsAny<IEnumerable<Guid>>(), It.IsAny<StandaloneGroupRequestDto>(), It.IsAny<Guid>(), It.IsAny<Guid>()));

            this._configurationWrapperMock = new Mock<IConfigurationWrapper>();

            this._groupService = new GroupService(
                this._mockedLikkleUoW.Object,
                this._mockedConfigurationProvider.Object,
                this._configurationWrapperMock.Object,
                this._geoCodingManagerMock.Object,
                this._subscrServiceMock.Object);
        }

        [TestMethod]
        public void InsertNewGroup_We_Can_Insert_New_Group()
        {
            // arrange
            var dbUser = new User() { Id = Guid.NewGuid() };
            var newArea = new Area() { Id = Guid.NewGuid() };

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
                TagIds = new List<Guid>() { FakeLikkleDbContext.GetAllAvailableTags().ToArray()[0].Key, FakeLikkleDbContext.GetAllAvailableTags().ToArray()[1].Key },
                AreaIds = new List<Guid>() { newArea.Id },
                UserId = dbUser.Id
            };

            // act
            var newGroupId = this._groupService.InsertNewGroup(newGroupRequest);

            // assert
            Assert.IsTrue(newGroupId != Guid.Empty);

            var newlyCreatedGroup = this._groupService.GetGroupById(newGroupId);

            Assert.IsNotNull(newlyCreatedGroup);
            this._subscrServiceMock.Verify(ssm => ssm.AutoSubscribeUsersFromExistingAreas(It.IsAny<IEnumerable<Guid>>(), It.IsAny<StandaloneGroupRequestDto>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
        }

        [TestMethod]
        public void GetUsersFromGroup_We_Can_Fetch_All_Users_From_Group()
        {
            // arrange
            var group = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { new User(), new User() } };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { group }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));

            // act
            var allUsersInGroup = this._groupService.GetUsersFromGroup(group.Id);

            // assert
            Assert.IsNotNull(allUsersInGroup);
            Assert.AreEqual(allUsersInGroup.Count(), 2);
        }

        [TestMethod]
        public void InsertNewGroup_StandaloneGroup_We_Create_Gets_Into_Our_HistoryGroups()
        {
            // arrange
            var dbUser = new User() { Id = Guid.NewGuid() };
            var newArea = new Area() { Id = Guid.NewGuid() };

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
                TagIds = new List<Guid>() { FakeLikkleDbContext.GetAllAvailableTags().ToArray()[0].Key, FakeLikkleDbContext.GetAllAvailableTags().ToArray()[1].Key },
                AreaIds = new List<Guid>() { newArea.Id },
                UserId = dbUser.Id
            };

            // act 
            var newGroupId = this._groupService.InsertNewGroup(newGroupRequest);

            // assert
            var user = this._mockedLikkleUoW.Object.UserRepository.GetUserById(dbUser.Id);
            Assert.IsNotNull(user.HistoryGroups);
            Assert.IsTrue(user.HistoryGroups.Select(hg => hg.GroupId).Contains(newGroupId));
            this._subscrServiceMock.Verify(ssm => ssm.AutoSubscribeUsersFromExistingAreas(It.IsAny<IEnumerable<Guid>>(), It.IsAny<StandaloneGroupRequestDto>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
        }

        [TestMethod]
        public void InserGroupAsNewArea_GroupAsNewArea_We_Create_Gets_Into_Our_HistoryGroups()
        {
            // arrange
            var dbUser = new User() { Id = Guid.NewGuid() };
            var newArea = new Area() { Id = Guid.NewGuid() };

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

            var newGroupRequest = new GroupAsNewAreaRequestDto() { UserId = dbUser.Id };

            // act 
            var newGroupId = this._groupService.InserGroupAsNewArea(newGroupRequest);

            // assert
            var user = this._mockedLikkleUoW.Object.UserRepository.GetUserById(dbUser.Id);
            Assert.IsNotNull(user.HistoryGroups);
            Assert.IsTrue(user.HistoryGroups.Select(hg => hg.GroupId).Contains(newGroupId));
        }

        [TestMethod]
        public void GetGroupCreationType_We_Can_Get_List_Of_Previously_Created_Groups_When_At_A_Place_With_Previous_Activity()
        {
            // arrange
            var dbUser = new User() { Id = Guid.NewGuid() };
            var groupOne = new Group() { Id = Guid.NewGuid(), IsActive = false };
            var groupTwo = new Group() { Id = Guid.NewGuid(), IsActive = true };
            var groupThree = new Group() { Id = Guid.NewGuid(), IsActive = false };

            dbUser.HistoryGroups = new List<HistoryGroup>() {
                new HistoryGroup()
                {
                    DateTimeGroupWasSubscribed = DateTime.UtcNow.AddDays(-2),
                    GroupId = groupOne.Id,
                    GroupThatWasPreviouslySubscribed = groupOne,
                    Id = Guid.NewGuid(),
                    UserId = dbUser.Id,
                    UserWhoSubscribedGroup = dbUser
                },
                new HistoryGroup()
                {
                    DateTimeGroupWasSubscribed = DateTime.UtcNow.AddDays(-2),
                    GroupId = groupThree.Id,
                    GroupThatWasPreviouslySubscribed = groupThree,
                    Id = Guid.NewGuid(),
                    UserId = dbUser.Id,
                    UserWhoSubscribedGroup = dbUser
                }
            };

            groupOne.Users = new List<User>() { dbUser };
            groupTwo.Users = new List<User>() { dbUser };
            groupThree.Users = new List<User>() { dbUser };

            var areaOne = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = 10.0000000,
                Longitude = 10.0000000,
                Radius = RadiusRangeEnum.FiftyMeters,
                IsActive = true,
                Groups = new List<Group>() { groupOne, groupTwo }
            };
            
            var areaTwo = new Area()
            {
                Id = Guid.NewGuid(),
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
            var result = this._groupService.GetGroupCreationType(10, 10, dbUser.Id);

            // assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.PrevousGroupsList);
            Assert.AreEqual(result.CreationType, CreateGroupActionTypeEnum.ListOfPreviouslyCreatedOrSubscribedGroups);
            Assert.AreEqual(2, result.PrevousGroupsList.Count());
        }

        [TestMethod]
        public void GetGroupCreationType_We_Get_AutomaticallyGroupAsNewArea_Response_When_Never_Created_Or_Subscribed_Group_Here()
        {
            // arrange 
            var dbUser = new User() { Id = Guid.NewGuid() };

            // act
            var result = this._groupService.GetGroupCreationType(20, 20, dbUser.Id);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.CreationType, CreateGroupActionTypeEnum.AutomaticallyGroupAsNewArea);
        }

        [TestMethod]
        public void GetGroupCreationType_We_Get_Choice_Screen_When_Creating_A_group_In_Existing_Active_Area()
        {
            // arrange
            var dbUser = new User() { Id = Guid.NewGuid() };

            var groupOne = new Group() { Id = Guid.NewGuid(), IsActive = true };
            var groupTwo = new Group() { Id = Guid.NewGuid(), IsActive = true };
            var newArea = new Area()

            {
                Id = Guid.NewGuid(),
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
            var result = this._groupService.GetGroupCreationType(10, 10, dbUser.Id);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.CreationType, CreateGroupActionTypeEnum.ChoiceScreen);
        }

        [TestMethod]
        public void ActivateGroup_We_Can_Activate_Previously_Not_Active_Group()
        {
            // arrange
            var dbUser = new User() { Id = Guid.NewGuid(), Groups = new List<Group>() };       
            var groupOne = new Group() { Id = Guid.NewGuid(), IsActive = true };
            
            var newArea = new Area();          
            var secondNewArea = new Area();

            groupOne.Areas = new List<Area>() { newArea, secondNewArea };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne },
                Users = new FakeDbSet<User>() { dbUser }
            }
            .Seed();
            
            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));

            // act
            this._groupService.ActivateGroup(groupOne.Id, dbUser.Id);

            // assert
            Assert.IsTrue(groupOne.IsActive);
            Assert.IsTrue(newArea.IsActive);
            Assert.IsTrue(secondNewArea.IsActive);
            Assert.IsTrue(dbUser.Groups.Any());
            Assert.IsTrue(dbUser.Groups.Any(gr => gr.Id == groupOne.Id));
            this._subscrServiceMock.Verify(ssm => ssm.AutoSubscribeUsersForRecreatedGroup(It.IsAny<IEnumerable<Guid>>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
        }
    }
}
