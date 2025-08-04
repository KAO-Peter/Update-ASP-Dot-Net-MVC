using System.Configuration;
using YoungCloud.Configurations.Exceptions;
using YoungCloud.Configurations.Runtime.Remoting;

namespace YoungCloud.Configurations
{
    /// <summary>
    /// The utility for configuration file.
    /// </summary>
    public class ConfigurationUtility
    {
        /// <summary>
        /// To get the mapped configuration instance.
        /// </summary>
        /// <param name="configFilename">The configuration file name.</param>
        /// <returns>The instance of configuration.</returns>
        public static Configuration Get(string configFilename)
        {
            if (string.IsNullOrEmpty(configFilename))
            {
                return Get();
            }
            ExeConfigurationFileMap _Map = new ExeConfigurationFileMap();
            _Map.ExeConfigFilename = configFilename;
            return ConfigurationManager.OpenMappedExeConfiguration(_Map, ConfigurationUserLevel.None);
        }

        /// <summary>
        /// To get the configuration instance.
        /// </summary>
        /// <returns>The instance of configuration.</returns>
        public static Configuration Get()
        {
            return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }



        /// <summary>
        /// To get the settings of exception handler.(Using "ExceptionHandlerSection" as default section name)
        /// </summary>
        /// <returns><see cref="ExceptionHandlerSection">ExceptionHandlerSection</see>.</returns>
        public static ExceptionHandlerSection GetExceptionHandlerSection()
        {
            return GetExceptionHandlerSection("ExceptionHandlerSection");
        }

        /// <summary>
        /// To get the settings of exception handler.
        /// </summary>
        /// <param name="sectionName">The name of exception handler section.</param>
        /// <returns><see cref="ExceptionHandlerSection">ExceptionHandlerSection</see>.</returns>
        public static ExceptionHandlerSection GetExceptionHandlerSection(string sectionName)
        {
            return Get().GetSection(sectionName) as ExceptionHandlerSection;
        }

        /// <summary>
        /// To get the settings of exception handler.
        /// </summary>
        /// <param name="sectionName">The name of exception handler section.</param>
        /// <param name="configFilename">The configuration file name.</param>
        /// <returns><see cref="ExceptionHandlerSection">ExceptionHandlerSection</see>.</returns>
        public static ExceptionHandlerSection GetExceptionHandlerSection(string sectionName, string configFilename)
        {
            return Get(configFilename).GetSection(sectionName) as ExceptionHandlerSection;
        }

        /// <summary>
        /// To get the settings of remoting service.(Using "RemotingServiceSection" as default section name)
        /// </summary>
        /// <returns><see cref="RemotingServiceSection">RemotingServiceSection</see>.</returns>
        public static RemotingServiceSection GetRemotingServiceSection()
        {
            return GetRemotingServiceSection("RemotingServiceSection");
        }

        /// <summary>
        /// To get the settings of remoting service.
        /// </summary>
        /// <param name="sectionName">The name of remoting service section.</param>
        /// <returns><see cref="RemotingServiceSection">RemotingServiceSection</see>.</returns>
        public static RemotingServiceSection GetRemotingServiceSection(string sectionName)
        {
            return Get().GetSection(sectionName) as RemotingServiceSection;
        }

        /// <summary>
        /// To get the settings of remoting service.
        /// </summary>
        /// <param name="sectionName">The name of remoting service section.</param>
        /// <param name="configFilename">The configuration file name.</param>
        /// <returns><see cref="RemotingServiceSection">RemotingServiceSection</see>.</returns>
        public static RemotingServiceSection GetRemotingServiceSection(string sectionName, string configFilename)
        {
            return Get(configFilename).GetSection(sectionName) as RemotingServiceSection;
        }

        /// <summary>
        /// To get the settings of remoting services.(Using "RemotingServicesSection" as default section name)
        /// </summary>
        /// <returns><see cref="RemotingServicesSection">RemotingServicesSection</see>.</returns>
        public static RemotingServicesSection GetRemotingServicesSection()
        {
            return GetRemotingServicesSection("RemotingServicesSection");
        }

        /// <summary>
        /// To get the settings of remoting services.
        /// </summary>
        /// <param name="sectionName">The name of remoting services section.</param>
        /// <returns><see cref="RemotingServicesSection">RemotingServicesSection</see>.</returns>
        public static RemotingServicesSection GetRemotingServicesSection(string sectionName)
        {
            return Get().GetSection(sectionName) as RemotingServicesSection;
        }

        /// <summary>
        /// To get the settings of remoting services.
        /// </summary>
        /// <param name="sectionName">The name of remoting services section.</param>
        /// <param name="configFilename">The configuration file name.</param>
        /// <returns><see cref="RemotingServicesSection">RemotingServicesSection</see>.</returns>
        public static RemotingServicesSection GetRemotingServicesSection(string sectionName, string configFilename)
        {
            return Get(configFilename).GetSection(sectionName) as RemotingServicesSection;
        }
    }
}