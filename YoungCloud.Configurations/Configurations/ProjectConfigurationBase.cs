using YoungCloud.Configurations.AppSettings;
using YoungCloud.Configurations.Db;
using YoungCloud.Configurations.Runtime.Remoting;

namespace YoungCloud.Configurations
{
    /// <summary>
    /// Gets or sets Medica8.com configurations.
    /// </summary>
    public abstract class ProjectConfigurationBase : ClassBase, IProjectConfiguration
    {
        /// <summary>
        /// Gets or sets the database configuration.
        /// </summary>
        /// <value>
        /// The database configuration.
        /// </value>
        public IDatabasesConfiguration DatabaseConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the application setting configuration.
        /// </summary>
        /// <value>
        /// The application setting configuration.
        /// </value>
        public IAppSettingsConfiguration AppSettingConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the remoting services configuration.
        /// </summary>
        /// <value>
        /// The remoting services configuration.
        /// </value>
        public IRemotingServicesConfiguration RemotingServicesConfiguration { get; set; }

    }
}