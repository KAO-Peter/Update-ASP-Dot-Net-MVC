using System.Configuration;

namespace YoungCloud.Configurations.Db
{
    /// <summary>
    /// The defintion of database configuration object.
    /// </summary>
    public interface IDatabasesConfiguration : IYoungCloudConfiguration
    {
        /// <summary>
        /// Gets or sets the main connection string. If the project only use one connection, you can use this.
        /// </summary>
        ConnectionStringSettings MainConnection { get; }

        /// <summary>
        /// Gets or sets the primary connection string. If the project's database applied replica feature, please use this connection.
        /// </summary>
        ConnectionStringSettings PrimaryConnection { get;  }

        /// <summary>
        /// Gets or sets the secondary connection string. If the project's database is applied replica feature, when the primary db is failed, you can use this connection string to avoid interruption service.
        /// </summary>
        ConnectionStringSettings SecondaryConnection { get;  }

        /// <summary>
        /// Gets or sets the master connection string. If project's db is applied backup-set feature, please use this connection string.
        /// </summary>
        ConnectionStringSettings MasterConnection { get;  }

        /// <summary>
        /// Gets or sets the slave connection string. If project's db is applied backup-set feature, when the master db is failed, you can use this connection string to avoid interruption service.
        /// </summary>
        ConnectionStringSettings SlaveConnection { get;  }

        /// <summary>
        /// Gets or sets the MSSQL connection.
        /// </summary>
        /// <value>
        /// The ms SQL connection.
        /// </value>
        ConnectionStringSettings MSSqlConnection { get;  }
    }
}