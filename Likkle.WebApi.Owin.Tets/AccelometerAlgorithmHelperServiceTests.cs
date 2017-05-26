using System;
using Likkle.BusinessEntities.Enums;
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
    public class AccelometerAlgorithmHelperServiceTests
    {
        private readonly IAccelometerAlgorithmHelperService _accelometerAlgorithmHelperService;

        private readonly Mock<ILikkleUoW> _mockedLikkleUoW;
        private readonly Mock<IConfigurationWrapper> _configurationWrapperMock;

        public AccelometerAlgorithmHelperServiceTests()
        {
            var fakeDbContext = new FakeLikkleDbContext().Seed();

            this._mockedLikkleUoW = new Mock<ILikkleUoW>();
            this._configurationWrapperMock = new Mock<IConfigurationWrapper>();
            this._configurationWrapperMock.Setup(x => x.PersonWalkingSpeedInKmh).Returns(5);

            this._accelometerAlgorithmHelperService = new AccelometerAlgorithmHelperService(
                this._mockedLikkleUoW.Object, 
                this._configurationWrapperMock.Object);
        }

        [TestMethod]
        public void We_Can_Get_Seconds_To_Closest_Boundary()
        {
            // arrange
            var smallAreaId = Guid.NewGuid();
            var smallArea = new Area()
            {
                Id = smallAreaId,
                Latitude = 42.626229,
                Longitude = 23.38143,
                IsActive = true,
                Radius = RadiusRangeEnum.FiftyMeters
            };

            var biggerAreaId = Guid.NewGuid();
            var biggerArea = new Area()
            {
                Id = biggerAreaId,
                Latitude = 42.62523,
                Longitude = 23.381371,
                IsActive = true,
                Radius = RadiusRangeEnum.FiveHundredMeters
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Areas = new FakeDbSet<Area>() { smallArea, biggerArea }
            }
            .Seed();
            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));

            // act
            var secondsToClosestBoundary = this._accelometerAlgorithmHelperService.SecondsToClosestBoundary(42.626391, 23.381071);

            // assert
            Assert.AreEqual(11.17049703433503, secondsToClosestBoundary);
        }
    }
}
