using System.Configuration;

namespace YoungCloud.Configurations.Exceptions
{
    /// <summary>
    /// The event log setting part of <see cref="ExceptionHandlerSection">ExceptionHandler section</see>.
    /// </summary>
    public class EventLogElement : ConfigurationElement
    {
        /// <summary>
        /// The caption of event log.
        /// </summary>
        [ConfigurationProperty("Caption", IsRequired = true)]
        public string Caption
        {
            get
            {
                return (string)this["Caption"];
            }
        }

        /// <summary>
        /// The event log has been enabled or not.
        /// </summary>
        [ConfigurationProperty("Enabled", DefaultValue = true)]
        public bool Enabled
        {
            get
            {
                return (bool)this["Enabled"];
            }
            set
            {
                this["Enabled"] = value;
            }
        }

        /// <summary>
        /// The Location of event log.
        /// </summary>
        [ConfigurationProperty("Location", IsRequired = true)]
        public string Location
        {
            get
            {
                return (string)this["Location"];
            }
        }
    }
}