using System.Configuration;

namespace YoungCloud.Configurations.Runtime.Remoting
{
    /// <summary>
    /// The configuration section of remoting service (server side).
    /// </summary>
    public class RemotingServiceSection : ConfigurationSection
    {
        /// <summary>
        /// The name of channel.
        /// </summary>
        [ConfigurationProperty("ChannelName", IsRequired = true)]
        public string ChannelName
        {
            get
            {
                return (string)this["ChannelName"];
            }
        }

        /// <summary>
        /// The machine name of channel.
        /// </summary>
        [ConfigurationProperty("MachineName", IsRequired = true)]
        public string MachineName
        {
            get
            {
                return (string)this["MachineName"];
            }
        }

        /// <summary>
        /// The name of service.
        /// </summary>
        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this["Name"];
            }
        }

        /// <summary>
        /// The port number of channel.
        /// </summary>
        [ConfigurationProperty("Port", IsRequired = true)]
        public int Port
        {
            get
            {
                return (int)this["Port"];
            }
        }

        /// <summary>
        /// The timeout setting of channel.
        /// </summary>
        [ConfigurationProperty("Timeout")]
        public int Timeout
        {
            get
            {
                return (int)this["Timeout"];
            }
        }

        /// <summary>
        /// The uri of service.
        /// </summary>
        [ConfigurationProperty("Uri", IsRequired = true)]
        public string Uri
        {
            get
            {
                return (string)this["Uri"];
            }
        }
    }
}