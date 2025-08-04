namespace YoungCloud.Configurations.AppSettings
{
    /// <summary>
    /// The defintion of appsettings configuration object.
    /// </summary>
    public interface IAppSettingsConfiguration : IYoungCloudConfiguration
    {
        void InitializeAppSettings();

        void InitializeAppSettings(string configFilePath);

        string GetAppSetting(string key);
    }
}