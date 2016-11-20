using System;
using System.Reflection;
using log4net;

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
    }
}