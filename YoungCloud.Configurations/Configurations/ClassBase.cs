using Autofac;
using YoungCloud.Configurations.AutofacSettings;

namespace YoungCloud.Configurations
{
    /// <summary>
    /// All class base.
    /// </summary>
    public abstract partial class ClassBase
    {
        protected ILifetimeScope _scope = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassBase"/> class.
        /// </summary>
        public ClassBase()
        {
            _scope = AutofacInitializer.Container.BeginLifetimeScope();
        }
    }
}