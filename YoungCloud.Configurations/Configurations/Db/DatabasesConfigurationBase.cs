using System.Configuration;

namespace YoungCloud.Configurations.Db
{
    public class DatabasesConfigurationBase : ClassBase, IDatabasesConfiguration
    {
        protected bool _isInitialized = false;

        public DatabasesConfigurationBase()
        {
            this.InitializeConnections();
        }

        protected virtual void InitializeConnections()
        {
            if (_isInitialized)
                return;

            foreach (ConnectionStringSettings connection in ConfigurationManager.ConnectionStrings)
            {
                switch (connection.Name)
                {
                    case "MainConnection":
                        this.MainConnection = connection;
                        break;

                    case "PrimaryConnection":
                        this.PrimaryConnection = connection;
                        break;

                    case "SecondaryConnection":
                        this.SecondaryConnection = connection;
                        break;

                    case "MasterConnection":
                        this.MasterConnection = connection;
                        break;

                    case "SlaveConnection":
                        this.SlaveConnection = connection;
                        break;

                    case "MSSqlConnection":
                        this.MSSqlConnection = connection;
                        break;

                    default:
                        break;
                }
            }

            _isInitialized = true;
        }

        public ConnectionStringSettings MainConnection
        {
            get;
            protected set;
        }

        public ConnectionStringSettings PrimaryConnection
        {
            get;
            protected set;
        }

        public ConnectionStringSettings SecondaryConnection
        {
            get;
            protected set;
        }

        public ConnectionStringSettings MasterConnection
        {
            get;
            protected set;
        }

        public ConnectionStringSettings SlaveConnection
        {
            get;
            protected set;
        }

        public ConnectionStringSettings MSSqlConnection
        {
            get;
            protected set;
        }
    }
}