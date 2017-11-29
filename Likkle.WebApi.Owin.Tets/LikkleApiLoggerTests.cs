using Likkle.BusinessServices;
using Likkle.WebApi.Owin.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace Likkle.WebApi.Owin.Tets
{
    [TestClass]
    public class LikkleApiLoggerTests
    {
        private readonly Mock<IConfigurationWrapper> _configurationWrapperMock;
        private readonly Mock<IMailService> _mailServiceMock;

        public LikkleApiLoggerTests()
        {
            this._configurationWrapperMock = new Mock<IConfigurationWrapper>();
            this._mailServiceMock = new Mock<IMailService>();
        }

        [TestMethod]
        public void We_Send_System_Exception_On_Email_If_Configured_In_Settings()
        {
            // arrange
            this._configurationWrapperMock.Setup(cwm => cwm.MailSupportOnException).Returns(true);

            // act
            var logger = new LikkleApiLogger(this._configurationWrapperMock.Object, this._mailServiceMock.Object);
            logger.OnActionException(
                new HttpActionContext(
                    new HttpControllerContext()
                    {
                        Request = new HttpRequestMessage()
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new System.Uri("https://www.google.bg/")
                        }
                    }, 
                    new ReflectedHttpActionDescriptor() { }
                ), 
                new System.Exception());

            // assert
            this._mailServiceMock.Verify(msm => msm.SendEmailForThrownException(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
