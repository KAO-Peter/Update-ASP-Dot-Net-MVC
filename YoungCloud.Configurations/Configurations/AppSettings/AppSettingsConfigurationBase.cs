using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;

namespace YoungCloud.Configurations.AppSettings
{
    public partial class AppSettingsConfigurationBase : IAppSettingsConfiguration
    {
        public AppSettingsConfigurationBase()
        {
            InitializeAppSettings();
        }

        public AppSettingsConfigurationBase(string configFilePath)
        {
            InitializeAppSettings(configFilePath);
        }

        public void InitializeAppSettings()
        {
            this.AppSettings = ConfigurationManager.AppSettings;
        }

        public void InitializeAppSettings(string configFilePath)
        {
            if (!File.Exists(configFilePath))
                throw new FileNotFoundException("Cannot be found the configuration file: " + configFilePath);

            Configuration _config = ConfigurationManager.OpenExeConfiguration(configFilePath);
            this.SpecificAppSettings = _config.AppSettings;
        }

        public virtual string GetAppSetting(string key)
        {
            if (AppSettings != null)
                return AppSettings[key];

            if (SpecificAppSettings != null)
                return SpecificAppSettings.Settings[key].Value;

            throw new NotImplementedException("AppSetting section is not initialized.");
        }

        protected AppSettingsSection SpecificAppSettings { get; set; }
        protected NameValueCollection AppSettings { get; set; }
    }
}
