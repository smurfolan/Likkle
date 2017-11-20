using System;
using System.Configuration;

namespace Likkle.BusinessServices
{
    public class ConfigurationWrapper : IConfigurationWrapper
    {
        public bool AutomaticallyCleanupGroupsAndAreas => Convert.ToBoolean(ConfigurationManager.AppSettings["automaticallyCleanupGroupsAndAreas"]);
        public string GoogleApiKeyForReverseGeoCoding => ConfigurationManager.AppSettings["googleApiKeyForReverseGeoCoding"];
        public string GoogleMapsApiRoot => ConfigurationManager.AppSettings["googleMapsApiRoot"];
        public string NumverifyApiRoot => ConfigurationManager.AppSettings["numverifyApiRoot"];
        public string NumverifyApiKey => ConfigurationManager.AppSettings["numverifyApiKey"];
        public int PersonWalkingSpeedInKmh => Convert.ToInt32(ConfigurationManager.AppSettings["personWalkingSpeedInKmh"]);
        public bool MailSupportOnException => Convert.ToBoolean(ConfigurationManager.AppSettings["mailSupportOnException"]);
        public string SupportEmail => ConfigurationManager.AppSettings["supportEmail"];
        public string SmtpClientHost => ConfigurationManager.AppSettings["smtpClientHost"];
        public string SupportEmailPassword => ConfigurationManager.AppSettings["supportEmailPassword"];
        public string HostingEnvironment => ConfigurationManager.AppSettings["hostingEnvironment"]; 
    }
}
