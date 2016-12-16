using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Likkle.BusinessEntities;
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
    public class DataServiceTests
    {
        private readonly DataService _dataService;
        private readonly Mock<ILikkleUoW> _mockedLikkleUoW;
        private readonly Mock<IConfigurationProvider> _mockedConfigurationProvider;

        public DataServiceTests()
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
        public void We_Can_Relate_User_To_Groups()
        {
            // arrange

            var currentlyWorkingLat = 10;
            var currentlyWorkingLon = currentlyWorkingLat;

            var group1 = Guid.NewGuid();
            var group2 = Guid.NewGuid();

            // 0. Add new area
            var newAreaId = this._dataService.InsertNewArea(new NewAreaRequest()
            {
                Longitude = 10,
                Latitude = 10,
                Radius = RadiusRangeEnum.FiftyMeters
            });

            // 0.5 Add groups
            var firstGroupId = this._dataService.InsertNewGroup(new StandaloneGroupRequestDto()
            {
                AreaIds = new List<Guid>()
                {
                    newAreaId
                },
                Name = "Group1",
                TagIds = new List<Guid>()
                {

                }
            });

            // 1. Add user
            var newUserId = this._dataService.InsertNewUser(new NewUserRequestDto()
            {
                FirstName = "Stefcho",
                LastName = "Stefchev",
                Email = "mail@mail.ma",
                IdsrvUniqueId = Guid.NewGuid().ToString()
            });

            // 2. Add groups that have specific coordinates and assign them to user
            // This means that in a specific x, y area this user subscribes these groups

            // 3. Create RelateUserToGroupsDto(Id = newUserId) request with coordinates x, y and groups in the same area
            // This means we are trying to change user's the groups in a specific x,y region
            var relateUserToGroupsRequest = new RelateUserToGroupsDto()
            {
                UserId = newUserId,
                Latitude = currentlyWorkingLat,
                Longitude = currentlyWorkingLon,
                GroupsUserSubscribes = new List<Guid>()
                {
                    group1, group2
                }
            };
            // act
            this._dataService.RelateUserToGroups(relateUserToGroupsRequest);

            var userSubscribtionsAroundCoordintes = this._dataService
                .GetUserSubscriptions(newUserId, currentlyWorkingLat, currentlyWorkingLon);


            // Assume our user has no groups he subscribes in this area
            // After applying step 3 we assume that the user now subscribes 2 groups

            // After that we make another RelateUserToGroupsDto request that passes one of the current user groups and some other
            // Assert that the old one is still there and the new ones are added

            // After that we make a request that is for the same coordinates but with completely new groups
            // Assert non of the old groups is still there

            // assert
        }
    }
}
