using Autofac;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace YoungCloud.Configurations.AutofacSettings
{
    /// <summary>
    /// Autofac initializer. it will be register all common setting using scaning assemblies.
    /// </summary>
    public partial class AutofacInitializer
    {
        /// <summary>
        /// The autofac register hendler.
        /// </summary>
        public static Action<ContainerBuilder> AutofacRegisterHendler = null;

        /// <summary>
        /// Gets or sets the container.
        /// </summary>
        public static IContainer Container { get; set; }

        public static ContainerBuilder CreateBuilder()
        {
            return new ContainerBuilder();
        }

        /// <summary>
        /// Initializes the register. But didn't build.
        /// </summary>
        /// <returns></returns>
        public static ContainerBuilder InitializeRegister()
        {
            // Create the builder with which components/services are registered.
            ContainerBuilder _builder = CreateBuilder();
            // Get all assemblies of YoungCloud.
            IList<Assembly> _assemblies = GetAllAssemblies();
            
            // Note: AsImplementedInterfaces shoule be applied to one interface has only one implemented class (excpet unittest class).
            foreach (Assembly _assembly in _assemblies)
            {
                // Registers all assemblies of ameng as self.
                _builder.RegisterAssemblyTypes(_assembly).AsSelf().PropertiesAutowired();
                // Registers the specific assemblies as implemented interfaces.
                _builder.RegisterAssemblyTypes(_assembly).AsImplementedInterfaces().PropertiesAutowired();
            }

            foreach (Assembly assembly in _assemblies)
            {
                var _registers = assembly.GetTypes()
                    .Where(x => typeof(AutofacConfigurationBase).IsAssignableFrom(x));
                foreach (var register in _registers)
                {
                    AutofacRegisterHendler +=
                        ((AutofacConfigurationBase)Activator.CreateInstance(register)).InitializeRegister;
                }
            }

            // Invoke other register of autofac
            if (AutofacRegisterHendler != null)
                AutofacRegisterHendler.Invoke(_builder);

            return _builder;
        }

        private static IList<Assembly>  GetAllAssemblies()
        {
            IList<Assembly> _assemblies = new List<Assembly>();
            string path = AppDomain.CurrentDomain.DynamicDirectory;
                path = path ?? AppDomain.CurrentDomain.BaseDirectory;
            string[] _dlls = Directory.GetFiles(path, "YoungCloud*.dll", SearchOption.AllDirectories);
            foreach (string _dll in _dlls)
            {
                _assemblies.Add(Assembly.LoadFile(_dll));
            }

            _dlls = Directory.GetFiles(path, "HRPortal*.dll", SearchOption.AllDirectories);
            foreach (string _dll in _dlls)
            {
                _assemblies.Add(Assembly.LoadFile(_dll));
            }

            //var _assemblies = AppDomain.CurrentDomain.GetAssemblies()
            //    .Where(c => c.GetName().Name.StartsWith("YoungCloud"))
            //    .OrderBy(c => c.GetName().Name.Length).ToList();

            return _assemblies;
        }

        /// <summary>
        /// Configures the specified initialize. It will be executing building method.
        /// </summary>
        /// <param name="Initialize">The initialize.</param>
        /// <returns></returns>
        public static IContainer Configure(Func<ContainerBuilder> Initialize)
        {
            ContainerBuilder _builder = Initialize.Invoke();
            Container = _builder.Build();
            return Container;
        }
    }
}