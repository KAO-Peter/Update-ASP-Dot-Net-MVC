using System.Configuration;

namespace YoungCloud.Configurations.Exceptions
{
    /// <summary>
    /// The text file log setting part of <see cref="ExceptionHandlerSection">ExceptionHandler section</see>.
    /// </summary>
    public class TextFileLogElement : ConfigurationElement
    {
        /// <summary>
        /// The text file log has been enabled or not.
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
        /// The days to keep log file.
        /// </summary>
        [ConfigurationProperty("KeepDays", DefaultValue = 14)]
        public int KeepDays
        {
            get
            {
                return (int)this["KeepDays"];
            }
        }

        /// <summary>
        /// The path to save exception log.
        /// </summary>
        [ConfigurationProperty("Path", IsRequired = true)]
        public string Path
        {
            get
            {
                return (string)this["Path"];
            }
        }
    }
}