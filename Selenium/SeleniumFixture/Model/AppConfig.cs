using System;
using System.Configuration;

namespace SeleniumFixture.Model
{
    internal static class AppConfig
    {
        public static string Get(string name)
        {
            var configValue = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(configValue))
            {
                configValue = ConfigurationManager.AppSettings.Get(name);
            }
            return configValue;
        }
    }
}