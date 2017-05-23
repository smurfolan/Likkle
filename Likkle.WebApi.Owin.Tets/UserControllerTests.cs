using System;
using Likkle.WebApi.Owin.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Likkle.WebApi.Owin.Tets
{
    [TestClass]
    public class UserControllerTests
    {
        private readonly Mock<ILikkleApiLogger> _apiLogger;

        public UserControllerTests()
        {
            this._apiLogger = new Mock<ILikkleApiLogger>();
        }

        [TestMethod]
        public void Bad_Request_Is_Returned_When_Phone_Number_Invalid()
        {
            throw new NotImplementedException();
        }
    }
}
