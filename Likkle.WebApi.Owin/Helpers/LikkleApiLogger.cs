using System;
using System.Reflection;
using System.Web.Http.Controllers;
using log4net;
using Likkle.BusinessServices.Utils;

namespace Likkle.WebApi.Owin.Helpers
{
    public class LikkleApiLogger : ILikkleApiLogger
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

            this.LogError($"[{formattedActionException.ErrorId}]{formattedActionException.ErrorMessage}", ex);

            // TODO: Mail support person who is stated in the Web.config

            return
                $"(ErrID:{formattedActionException.ErrorId}) {formattedActionException.ErrorMessage} {formattedActionException.KindMessage}";
        }
    }
}