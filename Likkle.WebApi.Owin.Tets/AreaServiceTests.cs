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
        public void InserGroupAsNewArea_We_Can_Insert_Group_As_New_Area()
        {
            // arrange
            var dbUser = new User() { Id = Guid.NewGuid() };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Users = new FakeDbSet<User>() { dbUser }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.TagRepository).Returns(new TagRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));

            var allAreasCount = this._areaService.GetAllAreas().Count();

            Assert.AreEqual(allAreasCount, 0);

            // act
            var newGroupId = this._groupService.InserGroupAsNewArea(new GroupAsNewAreaRequestDto());

            // assert
            allAreasCount = this._areaService.GetAllAreas().Count();

            Assert.AreNotEqual(newGroupId, Guid.Empty);
            Assert.AreEqual(allAreasCount, 1);

            var newlyCreatedGroup = this._groupService.GetGroupById(newGroupId);
            Assert.IsNotNull(newlyCreatedGroup);
        }

        [TestMethod]
        public void GetMetadataForArea_We_Can_Get_AreaMetadata()
        {
            // arrange
            var workingTag = new Tag()
            {
                Id = FakeLikkleDbContext.GetAllAvailableTags().ToArray()[0].Key,
                Name = FakeLikkleDbContext.GetAllAvailableTags().ToArray()[0].Value
            };

            var groupTwo = new Group() { Id = Guid.NewGuid(), Users = new List<User>(), Tags = new List<Tag>() { workingTag } };
            
            var area = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupTwo }
            };
            
            groupTwo.Areas = new List<Area>() { area };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupTwo },
                Areas = new FakeDbSet<Area>() { area },
                Users = new FakeDbSet<User>() { new User() }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));

            // act
            var areaMetadata = this._areaService.GetMetadataForArea(10, 10, area.Id);

            // assert
            Assert.IsNotNull(areaMetadata);
            Assert.AreEqual(areaMetadata.TagIds.Count(), 1);
            Assert.AreEqual(areaMetadata.DistanceTo, 0);
        }

        [TestMethod]
        public void GetMultipleAreasMetadata_We_Can_Get_Metadata_For_Multiple_Areas()
        {
            // arrange
            var groupTwo = new Group() { Id = Guid.NewGuid(), Users = new List<User>() };
            var firstArea = new Area() { Id = Guid.NewGuid(), Latitude = 10, Longitude = 10 };
            
            var secondArea = new Area() { Id = Guid.NewGuid(), Latitude = 10.00001, Longitude = 10.00001 };
            
            groupTwo.Areas = new List<Area>() { firstArea, secondArea };
            
            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupTwo },
                Areas = new FakeDbSet<Area>() { firstArea, secondArea },
                Users = new FakeDbSet<User>() { new User() }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));

            // act
            var multipleAreasMetadataRequestDto = new MultipleAreasMetadataRequestDto()
            {
                AreaIds = new List<Guid>() { firstArea.Id, secondArea.Id },
                Latitude = 10,
                Longitude = 10
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
            var firstUser = new User()
            {
                Id = Guid.NewGuid(),
                //FirstName = "Stefcho",
                //LastName = "Stefchev",
                //Email = "mail@mail.ma",
                //IdsrvUniqueId = Guid.NewGuid().ToString()
            };
            
            var secondUser = new User()
            {
                Id = Guid.NewGuid(),
                //FirstName = "Other",
                //LastName = "Name",
                //Email = "null@null.bg",
                //IdsrvUniqueId = Guid.NewGuid().ToString()
            };

            var groupOne = new Group() { Id = Guid.NewGuid(), Name = "GroupOne", Users = new List<User>() { firstUser } };
            var groupTwo = new Group() { Id = Guid.NewGuid(), Name = "GroupTwo", Users = new List<User>() { firstUser, secondUser } };
            
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
                Areas = new FakeDbSet<Area>() { area },
                Users = new FakeDbSet<User>() { firstUser, secondUser }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));

            // act
            var usersFromArea = this._areaService.GetUsersFromArea(area.Id);

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
            var firstArea = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = 10,
                Longitude = 10,
                Radius = RadiusRangeEnum.FiftyMeters,
                IsActive = true
            };
            
            var secondArea = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = 10.000001,
                Longitude = 10.000001,
                Radius = RadiusRangeEnum.FiftyMeters,
                IsActive = true
            };
            
            var thirdArea = new Area()
            {
                Id = Guid.NewGuid(),
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
            Assert.IsTrue(areaIds.Contains(firstArea.Id));
            Assert.IsTrue(areaIds.Contains(secondArea.Id));
            Assert.IsFalse(areaIds.Contains(thirdArea.Id));
        }

        [TestMethod]
        public void We_Can_Get_Areas_Inside_Of_Which_We_Are()
        {
            // arrange
            var firstArea = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = 10,
                Longitude = 10,
                Radius = RadiusRangeEnum.FiftyMeters,
                IsActive = true
            };
            
            var secondArea = new Area()
            {
                Id = Guid.NewGuid(),
                Latitude = 10,
                Longitude = 10,
                Radius = RadiusRangeEnum.ThreeHundredMeters,
                IsActive = true
            };
            
            var thirdArea = new Area()
            {
                Id = Guid.NewGuid(),
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
            Assert.IsTrue(areaIds.Contains(firstArea.Id));
            Assert.IsTrue(areaIds.Contains(secondArea.Id));
            Assert.IsFalse(areaIds.Contains(thirdArea.Id));
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
