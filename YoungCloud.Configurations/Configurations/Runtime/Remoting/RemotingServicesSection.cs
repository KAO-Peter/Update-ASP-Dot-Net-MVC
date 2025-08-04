using System.Configuration;

namespace YoungCloud.Configurations.Runtime.Remoting
{
    /// <summary>
    /// The configuration section of remoting services (client side).
    /// </summary>
    public class RemotingServicesSection : ConfigurationSection
    {
        /// <summary>
        /// The collection of remoting service settings.
        /// </summary>
        [ConfigurationProperty("Services")]
        public RemotingServiceCollection Services
        {
            get { return (RemotingServiceCollection)this["Services"]; }
        }
    }
}