using System;
using System.Web.Http.Controllers;

namespace Likkle.BusinessServices.Utils
{
    public interface ILikkleApiLogger
    {
        void LogInfo(string message);
        void LogError(string message, Exception ex);
        string OnActionException(HttpActionContext httpRequest, Exception ex);
    }
}
