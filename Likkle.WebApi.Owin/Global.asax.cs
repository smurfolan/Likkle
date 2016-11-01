namespace Likkle.WebApi.Owin
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Uncomment log4net when added
            log4net.Config.XmlConfigurator.Configure();
            
            //GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
