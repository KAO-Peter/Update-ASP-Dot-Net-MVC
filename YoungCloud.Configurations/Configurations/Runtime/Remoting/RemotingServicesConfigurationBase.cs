using System.Configuration;

namespace YoungCloud.Configurations.Runtime.Remoting
{
    public partial class RemotingServicesConfigurationBase : ClassBase, IRemotingServicesConfiguration
    {
        public RemotingServicesConfigurationBase()
        {
            InitializeRemotingSerivces();
        }

        public void InitializeRemotingSerivces()
        {
            RemotingServicesSection _remotingServices = ConfigurationManager.GetSection("RemotingServicesSection") as RemotingServicesSection;
            if (_remotingServices != null)
                this.RemotingServicesSection = _remotingServices.Services;
        }

        /// <summary>
        /// The remoting service setting of Global sessions service.
        /// </summary>
        public RemotingServiceElement GlobalSession
        {
            get
            {
                return RemotingServicesSection.Get("GlobalSessionsService");
            }
        }

        /// <summary>
        /// The remoting service setting of Global caches service.
        /// </summary>
        public RemotingServiceElement GlobalCache
        {
            get
            {
                return RemotingServicesSection.Get("GlobalCachesService");
            }
        }

        /// <summary>
        /// The remoting service setting of RSA key provider service.
        /// </summary>
        public RemotingServiceElement RSAKeyProvider
        {
            get
            {
                return RemotingServicesSection.Get("RSAKeyProviderService");
            }
        }

        protected RemotingServiceCollection RemotingServicesSection { get; set; }
    }
}