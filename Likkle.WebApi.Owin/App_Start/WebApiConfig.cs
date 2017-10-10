using System;
using System.Net.Http.Headers;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.Application;
using System.IO;

namespace Likkle.WebApi.Owin
{
    public static class WebApiConfig
    {
        public static HttpConfiguration Register()
        {
            var config = new HttpConfiguration();
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.EnableCors();

            // clear the supported mediatypes of the xml formatter
            config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(
                new MediaTypeHeaderValue("application/json-patch+json"));

            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
            json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            json.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; // Useful for circular references. When we use Automapper sometimes circular references happen and stackoverflow happens.

            // Configure swagger
            config
                .EnableSwagger(c => {
                    // API Metatadata
                    c.SingleApiVersion("v1", "Boongaloo REST API")
                        .Description("A sample API for testing and prototyping Boongaloo mobile app related resources")
                        .TermsOfService("Some terms")
                        .Contact(cc => cc
                        .Name("Boongaloo development and support team")
                        .Url("https://kristianazmanov.wixsite.com/boongaloo"));

                    // API Authorization
                    c.OAuth2("oauth2")
                    .Description("OAuth2 Authorization Code Grant")
                    .Flow("Authorization code")
                    .AuthorizationUrl("https://boongalooidsrv.azurewebsites.net/identity")
                    .Scopes(scopes =>
                    {
                        scopes.Add("boongaloomanagement", "Read/write access to protected resources");
                    });

                    // Extract summary comments into the Swagger UI
                    var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    var commentsFileName = "BoongalooApiEndpointComments.XML";
                    var commentsFile = Path.Combine(baseDirectory, commentsFileName);

                    c.IncludeXmlComments(commentsFile);
                })
                .EnableSwaggerUi();

            return config;
        }
    }
}
