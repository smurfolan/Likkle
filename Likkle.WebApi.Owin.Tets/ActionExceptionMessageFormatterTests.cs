using Likkle.BusinessServices.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace Likkle.WebApi.Owin.Tets
{
    [TestClass]
    public class ActionExceptionMessageFormatterTests
    {
        public ActionExceptionMessageFormatterTests()
        {

        }

        [TestMethod]
        public void Exception_On_Get_Request_Is_Properly_formatted()
        {
            // arrange
            var action = new HttpActionContext(new HttpControllerContext()
            {
                Request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(@"http://localhost:54036/api/v1/areas/51BA94F2-CF55-E711-ABC5-001C42F4C661")
                } 
            }, new ReflectedHttpActionDescriptor());

            // act
            var result = ActionLevelExceptionManager.GetActionExceptionMessage(action);

            // assert
            Assert.AreEqual(@"Error while trying to perfrom GET request to /api/v1/areas/51BA94F2-CF55-E711-ABC5-001C42F4C661.", result.ErrorMessage);
            Assert.AreEqual(@"Sorry for the inconvinience. Our team was notified so you can try again later.", result.KindMessage);
            Assert.IsTrue(result.ErrorId != string.Empty);
        }

        [TestMethod]
        public void Exception_On_Post_Request_Is_Properly_formatted()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void Exception_On_Put_Request_Is_Properly_formatted()
        {
            throw new NotImplementedException();
        }
    }
}
