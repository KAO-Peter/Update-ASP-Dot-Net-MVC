using Autofac;
using System.Reflection;
using YoungCloud.Configurations.AutofacSettings;

namespace YoungCloud.Databases.Configurations.AutofacSettings
{
    public class AutofacRegister : AutofacConfigurationBase
    {
        /// <summary>
        /// Initializes the register.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public override void InitializeRegister(ContainerBuilder builder)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
        }
    }
}
