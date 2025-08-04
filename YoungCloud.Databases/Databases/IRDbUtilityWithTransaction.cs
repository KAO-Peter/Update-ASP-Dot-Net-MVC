using System.Data;

namespace YoungCloud.Databases
{
    /// <summary>
    /// The definition of database access utility base object that support transaction.
    /// </summary>
    public interface IRDbUtilityWithTransaction : IRDbUtility
    {
        /// <summary>
        /// To commit transaction.
        /// </summary>
        void Commit();
        /// <summary>
        /// To rollback transaction.
        /// </summary>
        void Rollback();
        /// <summary>
        /// Get the state of connection.
        /// </summary>
        ConnectionState State { get; }
    }
}