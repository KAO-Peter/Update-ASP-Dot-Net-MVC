
namespace YoungCloud.Databases
{
    public partial interface IRDbUtility
    {
        /// <summary>
        /// To execute update commands.
        /// </summary>
        /// <param name="commandInfos">The collection of ICommandInfo instance.</param>
        /// <returns>The effect data row count array of each command.</returns>
        int[] ExecuteBatchUpdate(ICommandInfoCollection commandInfos);
        /// <summary>
        /// To execute update commands.
        /// </summary>
        /// <param name="commandInfos">The array of ICommandInfo instance.</param>
        /// <returns>The effect data row count array of each command.</returns>
        int[] ExecuteBatchUpdate(ICommandInfo[] commandInfos);
    }
}