using Likkle.DataModel.Repositories;
using Likkle.DataModel.TestingPurposes;
using Likkle.DataModel.UnitOfWork;
using Moq;
using System;

namespace Likkle.WebApi.Owin.Tets
{
    public static class DataGenerator
    {
        private static string StsIssuerUrl = "https://boongaloocompanysts/identity";

        public static void SetupMockedRepositories(
            Mock<ILikkleUoW> uowMock, 
            FakeLikkleDbContext fakeContext)
        {
            uowMock.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(fakeContext));
            uowMock.Setup(uow => uow.UserRepository).Returns(new UserRepository(fakeContext));
            uowMock.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(fakeContext));
            uowMock.Setup(uow => uow.HistoryGroupRepository).Returns(new HistoryGroupRepository(fakeContext));
        }

        public static void SetupAreaUserAndGroupRepositories(
            Mock<ILikkleUoW> uowMock,
            FakeLikkleDbContext fakeContext)
        {
            uowMock.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(fakeContext));
            uowMock.Setup(uow => uow.UserRepository).Returns(new UserRepository(fakeContext));
            uowMock.Setup(uow => uow.AreaRepository).Returns(new AreaRepository(fakeContext));
        }

        public static void SetupUserAndGroupRepositories(
            Mock<ILikkleUoW> uowMock,
            FakeLikkleDbContext fakeContext)
        {
            uowMock.Setup(uow => uow.GroupRepository).Returns(new GroupRepository(fakeContext));
            uowMock.Setup(uow => uow.UserRepository).Returns(new UserRepository(fakeContext));
        }

        public static string GetUniequeStsID()
        {
            return $"{StsIssuerUrl}{Guid.NewGuid()}";
        }
    }
}
