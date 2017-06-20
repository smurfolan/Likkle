using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
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

            // ================= Area1 (GR1, GR2) ========
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
                Id = areaOneId, Latitude = 10, Longitude = 10, Groups = new List<Group>() { groupOne, groupTwo },
                Radius = RadiusRangeEnum.FiftyMeters, IsActive = true
            };
            // ================= Area1 (GR1, GR2) ========

            // ================= Area2 (GR3) ========
            var groupThreeId = Guid.NewGuid();
            var groupThree = new Group()
            {
                Id = groupThreeId, Name = "Group three", IsActive = true
            };

            var areaTwoId = Guid.NewGuid();
            var areaTwo = new Area()
            {
                Id = areaTwoId, Latitude = 20, Longitude = 20, Groups = new List<Group>() { groupThree },
                Radius = RadiusRangeEnum.FiftyMeters,
                IsActive = true
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
            Assert.AreEqual(2 , groupsDependingOnUserSettingsAroundCoordinates.Count());


            // act
            groupsDependingOnUserSettingsAroundCoordinates =
                this._subscriptionSettingsService.GroupsForUserAroundCoordinatesBasedOnUserSettings(userId, 15, 15);

            // assert
            Assert.AreEqual(0, groupsDependingOnUserSettingsAroundCoordinates.Count());

            // act
            groupsDependingOnUserSettingsAroundCoordinates =
                this._subscriptionSettingsService.GroupsForUserAroundCoordinatesBasedOnUserSettings(userId, 20, 20);

            // assert
            Assert.AreEqual(1, groupsDependingOnUserSettingsAroundCoordinates.Count());

            // act
            groupsDependingOnUserSettingsAroundCoordinates =
                this._subscriptionSettingsService.GroupsForUserAroundCoordinatesBasedOnUserSettings(userId, 25, 25);

            // assert
            Assert.AreEqual(0, groupsDependingOnUserSettingsAroundCoordinates.Count());
        }

        [TestMethod]
        public void We_Can_Get_Groups_Around_Coordinates_Based_On_Subscription_Settings_SubscribeAllWithTagOptionSet()
        {
            var allTags = this._mockedLikkleUoW.Object.TagRepository.GetAllTags().ToList();

            // arrange
            var userId = Guid.NewGuid();
            var user = new User()
            {
                Id = userId,
                NotificationSettings = new NotificationSetting()
                {
                    AutomaticallySubscribeToAllGroups = false,
                    AutomaticallySubscribeToAllGroupsWithTag = true,
                    Tags = allTags.Where(t => t.Name == "Sport" || t.Name == "Help").ToList()
                }
            };          

            // ================= Area1 (GR1, GR2) ========
            var groupOneId = Guid.NewGuid();
            var groupOne = new Group()
            {
                Id = groupOneId,
                Name = "Group one",
                IsActive = true,
                Tags = new List<Tag>()
                {
                    allTags.FirstOrDefault(t => t.Name == "Animals")
                } 
            };

            var groupTwoId = Guid.NewGuid();
            var groupTwo = new Group()
            {
                Id = groupTwoId,
                Name = "Group two",
                IsActive = true,
                Tags = new List<Tag>()
                {
                    allTags.FirstOrDefault(t => t.Name == "Sport")
                }
            };

            var areaOneId = Guid.NewGuid();
            var areaOne = new Area()
            {
                Id = areaOneId,
                Latitude = 10,
                Longitude = 10,
                Groups = new List<Group>() { groupOne, groupTwo }, IsActive = true, Radius = RadiusRangeEnum.FiftyMeters
            };
            // ================= Area1 (GR1, GR2) ========

            // ================= Area1 (GR3, GR4, GR5) ========
            var groupThreeId = Guid.NewGuid();
            var groupThree = new Group()
            {
                Id = groupThreeId,
                Name = "Group three",
                IsActive = true,
                Tags = new List<Tag>()
                {
                    allTags.FirstOrDefault(t => t.Name == "Help"),
                    allTags.FirstOrDefault(t => t.Name == "Sport")
                }
            };

            var groupFourId = Guid.NewGuid();
            var groupFour = new Group()
            {
                Id = groupFourId,
                Name = "Group four",
                IsActive = true,
                Tags = new List<Tag>()
                {
                    allTags.FirstOrDefault(t => t.Name == "University")
                }
            };

            var groupFiveId = Guid.NewGuid();
            var groupFive = new Group()
            {
                Id = groupFiveId,
                Name = "Group five",
                IsActive = true,
                Tags = new List<Tag>()
                {
                    allTags.FirstOrDefault(t => t.Name == "Help"),
                    allTags.FirstOrDefault(t => t.Name == "Animals")
                }
            };

            var areaTwoId = Guid.NewGuid();
            var areaTwo = new Area()
            {
                Id = areaTwoId,
                Latitude = 20,
                Longitude = 20,
                Groups = new List<Group>() { groupThree, groupFour, groupFive }, IsActive = true, Radius = RadiusRangeEnum.FiftyMeters
            };
            // ================= Area1 (GR3, GR4, GR5) ========

            var populatedDatabase = new FakeLikkleDbContext()
            {
                Groups = new FakeDbSet<Group>() { groupOne, groupTwo, groupThree, groupFour, groupFive },
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
            Assert.AreEqual(1, groupsDependingOnUserSettingsAroundCoordinates.Count());

            Assert.IsTrue(groupsDependingOnUserSettingsAroundCoordinates.FirstOrDefault() == groupTwoId);

            // act 
            groupsDependingOnUserSettingsAroundCoordinates =
                this._subscriptionSettingsService.GroupsForUserAroundCoordinatesBasedOnUserSettings(userId, 15, 15);

            // assert
            Assert.AreEqual(0 , groupsDependingOnUserSettingsAroundCoordinates.Count());

            // act 
            groupsDependingOnUserSettingsAroundCoordinates =
                this._subscriptionSettingsService.GroupsForUserAroundCoordinatesBasedOnUserSettings(userId, 20, 20);

            // assert
            Assert.AreEqual(2 , groupsDependingOnUserSettingsAroundCoordinates.Count());
            var observedGroups =
                this._mockedLikkleUoW.Object.GroupRepository.GetGroups()
                    .Where(gr => groupsDependingOnUserSettingsAroundCoordinates.Contains(gr.Id));

            Assert.IsTrue(observedGroups.Any(gr => gr.Tags.Any(igr => igr.Name == "Sport")) && observedGroups.Any(gr => gr.Tags.Any(igr => igr.Name == "Help")));
        }
    }
}
