using System.Linq;
using AutoMapper;
using Likkle.BusinessEntities.Enums;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessServices;
using Likkle.DataModel.Repositories;
using Likkle.DataModel.TestingPurposes;
using Likkle.DataModel.UnitOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Likkle.WebApi.Owin.Tets
{
    [TestClass]
    public class DataServiceTests
    {
        private readonly DataService _dataService;
        private readonly Mock<ILikkleUoW> _mockedLikkleUoW;
        private readonly Mock<IConfigurationProvider> _mockedConfigurationProvider;

        public DataServiceTests()
        {
            this._mockedLikkleUoW = new Mock<ILikkleUoW>();
            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository)
                .Returns(new AreaRepository(new FakeLikkleDbContext()));


            this._mockedConfigurationProvider = new Mock<IConfigurationProvider>();

            var mapConfiguration = new MapperConfiguration(cfg => {
                cfg.AddProfile<EntitiesMappingProfile>();
            });
            this._mockedConfigurationProvider.Setup(mc => mc.CreateMapper()).Returns(mapConfiguration.CreateMapper);

            this._dataService = new DataService(
                this._mockedLikkleUoW.Object, 
                this._mockedConfigurationProvider.Object);
        }

        [TestMethod]
        public void We_Can_Get_All_Areas()
        {
            // arrange
            this._dataService.InsertNewArea(new NewAreaRequest()
            {
                Latitude = 12,
                Longitude = 12,
                Radius = RadiusRangeEnum.FiftyMeters
            });

            // act
            var areas = this._dataService.GetAllAreas();

            // assert
            Assert.AreEqual(1, areas.Count());
        }
    }
}
