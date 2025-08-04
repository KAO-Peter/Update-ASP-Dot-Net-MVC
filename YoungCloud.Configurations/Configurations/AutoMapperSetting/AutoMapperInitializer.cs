using AutoMapper;
using System;
using System.Linq;

namespace YoungCloud.Configurations.AutoMapperSetting
{
    public partial class AutoMapperInitializer: ClassBase
    {
        public static void Configure()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.SourceMemberNamingConvention = new LowerUnderscoreNamingConvention();
                cfg.DestinationMemberNamingConvention = new PascalCaseNamingConvention();
                GetConfigurations(Mapper.Configuration);
            });
        }

        private static void GetConfigurations(AutoMapper.IConfiguration configuration)
        {
            var _assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(c => c.GetName().Name.StartsWith("YoungCloud"))
                .OrderBy(c => c.GetName().Name.Length);
            foreach (var assembly in _assemblies)
            {
                var profiles = assembly.GetTypes()
                    .Where(x => x != typeof(Profile) && 
                                typeof(Profile).IsAssignableFrom(x));
                foreach (var profile in profiles)
                {
                    configuration.AddProfile((Profile)Activator.CreateInstance(profile));
                }
            }

            _assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(c => c.GetName().Name.StartsWith("HRPortal"))
                .OrderBy(c => c.GetName().Name.Length);
            foreach (var assembly in _assemblies)
            {
                var profiles = assembly.GetTypes()
                    .Where(x => x != typeof(Profile) &&
                                typeof(Profile).IsAssignableFrom(x));
                foreach (var profile in profiles)
                {
                    configuration.AddProfile((Profile)Activator.CreateInstance(profile));
                }
            }
        }
    }
}
