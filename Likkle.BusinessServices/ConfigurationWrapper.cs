﻿using System;
using System.Configuration;

namespace Likkle.BusinessServices
{
    public class ConfigurationWrapper : IConfigurationWrapper
    {
        public bool AutomaticallyCleanupGroupsAndAreas => Convert.ToBoolean(ConfigurationManager.AppSettings["automaticallyCleanupGroupsAndAreas"]);
    }
}
