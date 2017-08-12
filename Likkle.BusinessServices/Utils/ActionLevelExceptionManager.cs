using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Script.Serialization;

namespace Likkle.BusinessServices.Utils
{
    public static class ActionLevelExceptionManager
    {
        public static ActionMethodExceptionHandledResponse GetActionExceptionMessage(HttpActionContext context)
        {
            var httpVerb = context.Request.Method;
            var endpointPath = context.Request.RequestUri.AbsolutePath;

            var errorId = DateTime.Now.Ticks.ToString("X");

            var requestBodyMessage = string.Empty;

            if (httpVerb == HttpMethod.Post || httpVerb == HttpMethod.Put)
            {
                var requestBody = context.ActionArguments;

                var requestData = new Dictionary<string, string>();

                var serializer = new JavaScriptSerializer();

                foreach (var entry in requestBody)
                {
                    requestData.Add(entry.Key, serializer.Serialize(entry.Value));
                }

                requestBodyMessage = $" with request body: {serializer.Serialize(requestData)}";
            }

            return new ActionMethodExceptionHandledResponse
            {
                ErrorMessage = $"Error while trying to perfrom {httpVerb} request to {endpointPath}{requestBodyMessage}.",
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
