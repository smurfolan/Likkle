using System;
using System.Net.Http;
using System.Web.Http.Filters;
using System.Web.Script.Serialization;

namespace Likkle.BusinessServices.Utils
{
    public static class ActionLevelExceptionManager
    {
        public static ActionMethodExceptionHandledResponse GetActionExceptionMessage(HttpActionExecutedContext context)
        {
            var httpVerb = context.Request.Method;
            var endpointPath = context.Request.RequestUri.AbsolutePath;

            var errorId = DateTime.Now.Ticks.ToString("X");

            var requestBodyMessage = string.Empty;

            if (httpVerb == HttpMethod.Post || httpVerb == HttpMethod.Put)
            {
                var requestBody = context.ActionContext.ActionArguments["request"];

                var requestBodySerialized = new JavaScriptSerializer().Serialize(requestBody);

                requestBodyMessage = $" with request body: {requestBodySerialized.ToString()}";
            }

            return new ActionMethodExceptionHandledResponse
            {
                ErrorMessage = $"Error while trying to perfrom {httpVerb} request to {endpointPath} {requestBodyMessage}.",
                KindMessage = "Sorry for the inconvinience. Our team was notified so you can try again later.",
                ErrorId = errorId
            };
        }
    }

    public class ActionMethodExceptionHandledResponse
    {
        public string ErrorMessage { get; set; }
        public string KindMessage { get; set; }
        public string ErrorId { get; set; }
    }
}
