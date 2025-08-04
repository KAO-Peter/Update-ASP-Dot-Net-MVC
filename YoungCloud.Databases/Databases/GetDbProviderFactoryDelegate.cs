using System.Data.Common;

namespace YoungCloud.Databases
{
    /// <summary>
    /// Get the DbProviderFactory implementation delegate.
    /// </summary>
    /// <returns>The instance of <see cref="DbProviderFactory">DbProviderFactory</see>.</returns>
    public delegate DbProviderFactory GetDbProviderFactoryDelegate();
}