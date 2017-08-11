using Likkle.BusinessServices.Utils;
using Likkle.WebApi.Owin.DI;
using Likkle.WebApi.Owin.Helpers;
using Ninject;
using System.Net.Http;
using System.Web.Http.Filters;

namespace Likkle.WebApi.Owin.Filters
{
    public class EndpointExceptionHandlingFilter : ExceptionFilterAttribute
    {
        private readonly ILikkleApiLogger _apiLogger;
        public EndpointExceptionHandlingFilter()
        {
            // NOTE: Used like this because it is a bit more complex to inject it through the constructor arguments.
            var kernel = NinjectConfig.Kernel;
            this._apiLogger = kernel.Get<ILikkleApiLogger>();
        }

        public override void OnException(HttpActionExecutedContext context)
        {
            var exceptionFormattedResponse = ActionLevelExceptionManager.GetActionExceptionMessage(context);

            // 1. Formatted client response
            context.Response = new HttpResponseMessage()
            {
                Content = new StringContent($"[ErrorId:{exceptionFormattedResponse.ErrorId}] {exceptionFormattedResponse.ErrorMessage}\n{exceptionFormattedResponse.KindMessage}", System.Text.Encoding.UTF8, "text/plain"),
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            };

            // 2. Log error
            _apiLogger.LogError($"[{exceptionFormattedResponse.ErrorId}] {exceptionFormattedResponse.ErrorMessage}", context.Exception);

            // 3. Mail support person
        }
    }
}