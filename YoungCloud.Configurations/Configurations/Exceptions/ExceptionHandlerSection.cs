using System.Configuration;

namespace YoungCloud.Configurations.Exceptions
{
    /// <summary>
    /// The configuration section of ExceptionHandler.
    /// </summary>
    public class ExceptionHandlerSection : ConfigurationSection
    {
        /// <summary>
        /// The name of application.
        /// </summary>
        [ConfigurationProperty("ApplicationName", IsRequired = true)]
        public string ApplicationName
        {
            get
            {
                return (string)this["ApplicationName"];
            }
        }
        
        /// <summary>
        /// The event log setting part of ExceptionHandler section.
        /// </summary>
        [ConfigurationProperty("EventLog", IsRequired = true)]
        public EventLogElement EventLog
        {
            get
            {
                return (EventLogElement)this["EventLog"];
            }
        }

        /// <summary>
        /// The handler uses asynchronous threads or not.
        /// </summary>
        [ConfigurationProperty("HandleByAsync", DefaultValue = true)]
        public bool HandleByAsync
        {
            get
            {
                return (bool)this["HandleByAsync"];
            }
            set
            {
                this["HandleByAsync"] = value;
            }
        }

        /// <summary>
        /// The application is running under debug mode or not.
        /// </summary>
        [ConfigurationProperty("IsDebug", DefaultValue = true)]
        public bool IsDebug
        {
            get
            {
                return (bool)this["IsDebug"];
            }
            set
            {
                this["IsDebug"] = value;
            }
        }

        /// <summary>
        /// The text file log setting part of ExceptionHandler section.
        /// </summary>
        [ConfigurationProperty("TextFileLog", IsRequired = true)]
        public TextFileLogElement TextFileLog
        {
            get
            {
                return (TextFileLogElement)this["TextFileLog"];
            }
        }
    }
}