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

            var mapConfiguration = new MapperConfiguration(cfg =>
            {
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
        public void GetUsersFromArea_We_Can_Get_Users_From_Area()
        {
            // arrange
            var firstUser = new User() { Id = Guid.NewGuid() };
            var secondUser = new User() { Id = Guid.NewGuid() };

            var groupOne = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { firstUser } };
            var groupTwo = new Group() { Id = Guid.NewGuid(), Users = new List<User>() { firstUser, secondUser } };

            var area = new Area()
            {
                Id = Guid.NewGuid(),
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
        public void GetAreas_We_Can_Get_Areas_In_A_Radius_Of_Certain_Point()
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
        public void GetAreas_We_Can_Get_Areas_Inside_Of_Which_We_Are()
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
        public void InsertNewArea_We_Can_Insert_New_Area()
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
            // arrange

            // act

            // assert
            throw new NotImplementedException();
        }

        [TestMethod]
        public void GetMultipleAreasMetadata_We_Can_Get_Multiple_Areas_Metadata()
        {
            // arrange
            var commonUser = new User() { Id = Guid.NewGuid() };

            var groupOne = new Group() {
                Id = Guid.NewGuid(),
                Users = new List<User>() { commonUser, new User()}
            };
            var groupTwo = new Group() {
                Id = Guid.NewGuid(),
                Users = new List<User>() { new User(), commonUser }
            };

            var firstApproxiateAddress = "Approximate address 1";
            var firstArea = new Area() {
                Id = Guid.NewGuid(),
                Latitude = 42.690292,
                Longitude = 23.319351,
                ApproximateAddress = firstApproxiateAddress,
                Groups = new List<Group>() { groupOne }
            };
            var secondApproxiateAddress = "Approximate address 2";
            var secondArea = new Area() {
                Id = Guid.NewGuid(),
                Latitude = 42.691542,
                Longitude = 23.319733,
                ApproximateAddress = secondApproxiateAddress,
                Groups = new List<Group>() { groupOne, groupTwo }
            };
            var thirdArea = new Area() {
                Id = Guid.NewGuid(),
                Latitude = 42.691177,
                Longitude = 23.318429,
                ApproximateAddress = "Approximate address 3"
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Areas = new FakeDbSet<Area>() { firstArea, secondArea, thirdArea }
            }
            .Seed();
            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));

            var request = new MultipleAreasMetadataRequestDto()
            {
                AreaIds = new List<Guid>() { firstArea.Id, secondArea.Id },
                Latitude = 42.606060,
                Longitude = 23.606060
            };
            // act
            var areasMetadata = this._areaService.GetMultipleAreasMetadata(request);

            // assert
            Assert.IsNotNull(areasMetadata);
            Assert.AreEqual(2, areasMetadata.Count());

            var firstExpectedArea = areasMetadata.FirstOrDefault(a => a.ApproximateAddress == firstApproxiateAddress);
            Assert.AreEqual(25272.1926358516, firstExpectedArea.DistanceTo);
            Assert.AreEqual(2, firstExpectedArea.NumberOfParticipants);

            var secondExpectedArea = areasMetadata.FirstOrDefault(a => a.ApproximateAddress == secondApproxiateAddress);
            Assert.AreEqual(25294.928915683049, secondExpectedArea.DistanceTo);
            Assert.AreEqual(3, secondExpectedArea.NumberOfParticipants);
        }

        [TestMethod]
        public void We_Can_Get_All_Users_Falling_Under_Specific_Areas()
        {
            // arrange
            var userOneId = Guid.NewGuid();
            var userOne = new User(){ Id = userOneId, Latitude = 10.000001, Longitude = 10.000001 };

            var userTwoId = Guid.NewGuid();
            var userTwo = new User(){ Id = userTwoId, Latitude = 10.000005, Longitude = 10.000005 };

            var areaId = Guid.NewGuid();
            var area = new Area()
            {
                Id = areaId, Latitude = 10.000012,Longitude = 10.000012, Radius = RadiusRangeEnum.FiftyMeters
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Areas = new FakeDbSet<Area>() { area },
                Users = new FakeDbSet<User>() { userOne, userTwo }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));

            // act
            var result = this._areaService.GetUsersFallingUnderSpecificAreas(new List<Guid>() { area.Id });

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Contains(userOneId));
            Assert.IsTrue(result.Contains(userTwoId));
        }
    }
}
