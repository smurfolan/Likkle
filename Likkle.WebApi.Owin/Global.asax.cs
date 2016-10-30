using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace Likkle.WebApi.Owin
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Uncomment log4net when added
            //log4net.Config.XmlConfigurator.Configure();
            //GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
