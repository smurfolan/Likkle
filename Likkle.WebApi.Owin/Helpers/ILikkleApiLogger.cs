using System;

namespace Likkle.WebApi.Owin.Helpers
{
    public interface ILikkleApiLogger
    {
        void LogInfo(string message);
        void LogError(string message, Exception ex);
    }
}
