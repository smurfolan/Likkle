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
    public class AreaServiceTests
    {
        private readonly IAreaService _areaService;
        private readonly IGroupService _groupService;

        private readonly Mock<ILikkleUoW> _mockedLikkleUoW;
        private readonly Mock<IConfigurationProvider> _mockedConfigurationProvider;
        private readonly Mock<IConfigurationWrapper> _configurationWrapperMock;
        private readonly Mock<IGeoCodingManager> _geoCodingManagerMock;
        private readonly Mock<ISubscriptionService> _subscrServiceMock;
        public AreaServiceTests()
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

            _geoCodingManagerMock = new Mock<IGeoCodingManager>();
            _geoCodingManagerMock.Setup(gcm => gcm.GetApproximateAddress(It.IsAny<NewAreaRequest>()))
                .Returns(Guid.NewGuid().ToString);

            _subscrServiceMock = new Mock<ISubscriptionService>();
            _subscrServiceMock.Setup(ssm => ssm.AutoSubscribeUsersFromExistingAreas(It.IsAny<IEnumerable<Guid>>(), It.IsAny<StandaloneGroupRequestDto>(), It.IsAny<Guid>(), It.IsAny<Guid>()));

            var mapConfiguration = new MapperConfiguration(cfg => {
                cfg.AddProfile<EntitiesMappingProfile>();
            });
            this._mockedConfigurationProvider.Setup(mc => mc.CreateMapper()).Returns(mapConfiguration.CreateMapper);

            this._configurationWrapperMock = new Mock<IConfigurationWrapper>();

            this._areaService = new AreaService(
                this._mockedLikkleUoW.Object,
                this._mockedConfigurationProvider.Object,
                this._configurationWrapperMock.Object,
                this._geoCodingManagerMock.Object);

            this._groupService = new GroupService(
                this._mockedLikkleUoW.Object,
                this._mockedConfigurationProvider.Object,
                this._configurationWrapperMock.Object,
                this._geoCodingManagerMock.Object,
                this._subscrServiceMock.Object);
        }

        [TestMethod]
        public void We_Can_Insert_Grou_As_New_Area()
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

            var allAreasCount = this._areaService.GetAllAreas().Count();

            Assert.AreEqual(allAreasCount, 0);

            // act
            var newGroupId = this._groupService.InserGroupAsNewArea(newGroupRequest);

            // assert
            allAreasCount = this._areaService.GetAllAreas().Count();

            Assert.AreNotEqual(newGroupId, Guid.Empty);
            Assert.AreEqual(allAreasCount, 1);

            var newlyCreatedGroup = this._groupService.GetGroupById(newGroupId);
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
                Id = groupOneId,
                Name = "GroupOne",
                Users = new List<User>(),
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
            var areaMetadata = this._areaService.GetMetadataForArea(myLocationLatitude, myLocationLongitude, areaId);

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

            var multipleAreasMetadata = this._areaService.GetMultipleAreasMetadata(multipleAreasMetadataRequestDto);

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
            var usersFromArea = this._areaService.GetUsersFromArea(areaId);

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
                Radius = RadiusRangeEnum.FiftyMeters,
                IsActive = true
            };

            var secondAreaId = Guid.NewGuid();
            var secondArea = new Area()
            {
                Id = secondAreaId,
                Latitude = 10.000001,
                Longitude = 10.000001,
                Radius = RadiusRangeEnum.FiftyMeters,
                IsActive = true
            };

            var thirdAreaId = Guid.NewGuid();
            var thirdArea = new Area()
            {
                Id = thirdAreaId,
                Latitude = 42,
                Longitude = 42,
                Radius = RadiusRangeEnum.FiftyMeters,
                IsActive = true
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Areas = new FakeDbSet<Area>() { firstArea, secondArea, thirdArea }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));

            // act
            var areasAroundCoordinatesInRadius = this._areaService.GetAreas(10, 10, 10);

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
                Radius = RadiusRangeEnum.FiftyMeters,
                IsActive = true
            };

            var secondAreaId = Guid.NewGuid();
            var secondArea = new Area()
            {
                Id = secondAreaId,
                Latitude = 10,
                Longitude = 10,
                Radius = RadiusRangeEnum.ThreeHundredMeters,
                IsActive = true
            };

            var thirdAreaId = Guid.NewGuid();
            var thirdArea = new Area()
            {
                Id = thirdAreaId,
                Latitude = 50,
                Longitude = 50,
                Radius = RadiusRangeEnum.FiftyMeters,
                IsActive = true
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Areas = new FakeDbSet<Area>() { firstArea, secondArea, thirdArea }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));

            // act
            var areasWhereWeFallIn = this._areaService.GetAreas(10, 10);

            // assert
            Assert.IsNotNull(areasWhereWeFallIn);
            Assert.AreEqual(areasWhereWeFallIn.Count(), 2);

            var areaIds = areasWhereWeFallIn.Select(a => a.Id);
            Assert.IsTrue(areaIds.Contains(firstAreaId));
            Assert.IsTrue(areaIds.Contains(secondAreaId));
            Assert.IsFalse(areaIds.Contains(thirdAreaId));
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
            var newAreaId = this._areaService.InsertNewArea(newAreaRequest);

            // assert
            var newlyCreatedArea = this._areaService.GetAreaById(newAreaId);

            Assert.IsNotNull(newlyCreatedArea);
        }

        [TestMethod]
        public void We_Can_Get_All_Areas_A_Group_Belongs_To()
        {
            throw new NotImplementedException();
        }
    }
}
