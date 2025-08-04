using YoungCloud.Configurations.AppSettings;
using YoungCloud.Configurations.Db;
using YoungCloud.Configurations.Runtime.Remoting;

namespace YoungCloud.Configurations
{
    /// <summary>
    /// Gets or sets the project configurations, include appkeys, db settings, remoting settings.
    /// </summary>
    public interface IProjectConfiguration
    {
        /// <summary>
        /// Gets or sets the application setting configuration.
        /// </summary>
        /// <value>
        /// The application setting configuration.
        /// </value>
        IAppSettingsConfiguration AppSettingConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the database configuration.
        /// </summary>
        /// <value>
        /// The database configuration.
        /// </value>
        IDatabasesConfiguration DatabaseConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the remoting services configuration.
        /// </summary>
        /// <value>
        /// The remoting services configuration.
        /// </value>
        IRemotingServicesConfiguration RemotingServicesConfiguration { get; set; }

    }
}