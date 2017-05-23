using System;
using System.Configuration;

namespace Likkle.BusinessServices
{
    public class ConfigurationWrapper : IConfigurationWrapper
    {
        public bool AutomaticallyCleanupGroupsAndAreas => Convert.ToBoolean(ConfigurationManager.AppSettings["automaticallyCleanupGroupsAndAreas"]);

        public string GoogleApiKeyForReverseGeoCoding => ConfigurationManager.AppSettings["googleApiKeyForReverseGeoCoding"];

        public string GoogleMapsApiRoot => ConfigurationManager.AppSettings["googleMapsApiRoot"];
    }
}
