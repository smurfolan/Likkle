using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
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
    public class SubscriptionSettingsServiceTests
    {
        private readonly ISubscriptionSettingsService _subscriptionSettingsService;

        private readonly Mock<ILikkleUoW> _mockedLikkleUoW;
        private readonly Mock<IConfigurationProvider> _mockedConfigurationProvider;

        public SubscriptionSettingsServiceTests()
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

            this._subscriptionSettingsService = new SubscriptionSettingsService(
                this._mockedLikkleUoW.Object,
                this._mockedConfigurationProvider.Object);
        }

        [TestMethod]
        public void We_Can_Get_Groups_Around_Coordinates_Based_On_Subscription_Settings_SubscribeAllAvailableOptionSet()
        {
            // arrange
            var userId = Guid.NewGuid();
            var user = new User()
            {
                Id = userId,
                NotificationSettings = new NotificationSetting()
                {
                    AutomaticallySubscribeToAllGroups = true,
                    AutomaticallySubscribeToAllGroupsWithTag = false
                }
            };

            var groupOneId = Guid.NewGuid();
            var groupOne = new Group()
            {
                Id = groupOneId, Name = "Group one", IsActive = true
            };

            var groupTwoId = Guid.NewGuid();
            var groupTwo = new Group()
            {
                Id = groupTwoId, Name = "Group two", IsActive = true
            };

            var areaOneId = Guid.NewGuid();
            var areaOne = new Area()
            {
                Id = areaOneId, Latitude = 10, Longitude = 10, Groups = new List<Group>() { groupOne, groupTwo }
            };
            
            var groupThreeId = Guid.NewGuid();
            var groupThree = new Group()
            {
                Id = groupThreeId, Name = "Group three", IsActive = true
            };

            var areaTwoId = Guid.NewGuid();
            var areaTwo = new Area()
            {
                Id = areaTwoId, Latitude = 20, Longitude = 20, Groups = new List<Group>() { groupThree }
            };

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo, groupThree },
                Areas = new FakeDbSet<Area>() { areaOne, areaTwo },
                Users = new FakeDbSet<User>() { user }
            }
            .Seed();

            this._mockedLikkleUoW.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(populatedDatabase));
            this._mockedLikkleUoW.Setup(uow => uow.UserRepository).Returns(new UserRepository(populatedDatabase));

            // act
            var groupsDependingOnUserSettingsAroundCoordinates =
                this._subscriptionSettingsService.GroupsForUserAroundCoordinatesBasedOnUserSettings(userId, 10, 10);

            // assert

            throw new NotImplementedException();
        }

        [TestMethod]
        public void We_Can_Get_Groups_Around_Coordinates_Based_On_Subscription_Settings_SubscribeAllWithTagOptionSet()
        {
            throw new NotImplementedException();
        }
    }
}
