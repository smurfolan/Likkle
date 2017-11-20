using System;
using System.Reflection;
using System.Web.Http.Controllers;
using log4net;
using Likkle.BusinessServices.Utils;
using Likkle.BusinessServices;
using System.Threading.Tasks;

namespace Likkle.WebApi.Owin.Helpers
{
    public class LikkleApiLogger : ILikkleApiLogger
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IConfigurationWrapper _configuration;
        private readonly IMailService _mailService;
        public LikkleApiLogger(
            IConfigurationWrapper config, 
            IMailService mailService)
        {
            this._configuration = config;
            this._mailService = mailService;
        }

        public void LogInfo(string message)
        {
            Log.Info(message);
        }

        public void LogError(string message, Exception ex)
        {
            Log.Error(message, ex);
        }

        public string OnActionException(HttpActionContext httpRequest, Exception ex)
        {
            var formattedActionException = ActionLevelExceptionManager.GetActionExceptionMessage(httpRequest);

            var mainErrorMessage = $"[{formattedActionException.ErrorId}]{formattedActionException.ErrorMessage}, {ex.Message}";

            this.LogError(mainErrorMessage, ex);

            if (this._configuration.MailSupportOnException)
                Task.Run(async () => await this._mailService.SendEmailForThrownException(this._configuration.SupportEmail, $"{mainErrorMessage} ---> Stack trace: {ex.StackTrace.ToString()}")); 
                    
            return
                $"(ErrID:{formattedActionException.ErrorId}) {formattedActionException.ErrorMessage} {formattedActionException.KindMessage}";
        }
    }
}