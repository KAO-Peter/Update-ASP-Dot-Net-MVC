using System.Configuration;

namespace YoungCloud.Configurations.Runtime.Remoting
{
    /// <summary>
    /// The settings of remoting service.
    /// </summary>
    public class RemotingServiceElement : ConfigurationElement
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
        /// The name of service.(This is key of setting)
        /// </summary>
        [ConfigurationProperty("Name", IsRequired = true, IsKey = true)]
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

        /// <summary>
        /// The url of service for Activator.
        /// </summary>
        public string Url
        {
            get
            {
                return string.Format("tcp://{0}:{1}/{2}", MachineName, Port, Uri);
            }
        }
    }
}