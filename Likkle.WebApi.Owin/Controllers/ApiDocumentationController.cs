﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Xml;
using Likkle.WebApi.Owin.Helpers;

namespace Likkle.WebApi.Owin.Controllers
{
    [RoutePrefix("api/v1/documentation")]
    public class ApiDocumentationController : ApiController
    {
        private readonly ILikkleApiLogger _apiLogger;

        public ApiDocumentationController(ILikkleApiLogger logger)
        {
            this._apiLogger = logger;
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult Get()
        {
            try
            {
                var result = ExtractDocumentationFromResourceFile();

                return Content(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                _apiLogger.LogError("Error while generating api documentation", ex);
                return InternalServerError();
            }

        }

        #region Helper methods and classes
        private class ApiDocumentationDto
        {
            public List<ApiMethodDescriptionDto> Methods { get; set; }
        }

        private class ApiMethodDescriptionDto
        {
            public string ExampleForARequest { get; set; }
            public List<ApiMethodParameterDescription> Parameters { get; set; }
            public string Returns { get; set; }

        }

        private class ApiMethodParameterDescription
        {
            public string ParameterName { get; set; }
            public string ParameterDescription { get; set; }
        }

        private ApiDocumentationDto ExtractDocumentationFromResourceFile()
        {
            var pathToResFile = GetXmlCommentsPath();

            var doc = new XmlDocument();
            doc.Load(pathToResFile);

            var apiMembers = doc.GetElementsByTagName("member");

            if (apiMembers == null)
                throw new Exception("Xml documentation resource file problem");

            var result = new ApiDocumentationDto()
            {
                Methods = new List<ApiMethodDescriptionDto>()
            };

            foreach (XmlNode member in apiMembers)
            {
                result.Methods.Add(ExtractMethodDescriptionDtoFromXmlMekup(member));
            }

            return result;
        }

        private ApiMethodDescriptionDto ExtractMethodDescriptionDtoFromXmlMekup(XmlNode member)
        {
            var result = new ApiMethodDescriptionDto();

            var summary = member.SelectSingleNode("summary");

            result.ExampleForARequest = summary?.InnerText.Trim() ?? "No summary available.";
            result.Parameters = new List<ApiMethodParameterDescription>();

            var parameters = member.SelectNodes("param");
            if (parameters != null && parameters.Count > 0)
            {
                foreach (var parameter in parameters)
                {
                    result.Parameters.Add(new ApiMethodParameterDescription()
                    {
                        ParameterName = ((System.Xml.XmlElement)parameter).Attributes[0].Value,
                        ParameterDescription = ((System.Xml.XmlElement)parameter).InnerText
                    });
                }
            }

            var returns = member.SelectSingleNode("returns");
            result.Returns = returns?.InnerText.Trim() ?? "No description for return value.";

            return result;
        }

        private static string GetXmlCommentsPath()
        {
            return $@"{AppDomain.CurrentDomain.BaseDirectory}\bin\XmlComments.xml";
        }
        #endregion
    }
}
