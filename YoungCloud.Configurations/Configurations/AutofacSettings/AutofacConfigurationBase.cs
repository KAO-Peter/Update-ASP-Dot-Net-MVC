using Autofac;

namespace YoungCloud.Configurations.AutofacSettings
{
    /// <summary>
    /// All Autofac register parent. If extend this, AutofacInitializer.Configrue will add to register table.
    /// </summary>
    public partial class AutofacConfigurationBase
    {
        /// <summary>
        /// Initializes the register. It should be used in downline project to register Autofac register table.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public virtual void InitializeRegister(ContainerBuilder builder)
        {
        }
    }
}
