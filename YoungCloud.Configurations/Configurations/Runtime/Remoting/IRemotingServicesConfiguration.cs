namespace YoungCloud.Configurations.Runtime.Remoting
{
    /// <summary>
    /// The defintion of remoting services configuration object.
    /// </summary>
    public interface IRemotingServicesConfiguration : IYoungCloudConfiguration
    {
        RemotingServiceElement GlobalSession { get; }

        RemotingServiceElement GlobalCache { get; }

        RemotingServiceElement RSAKeyProvider { get; }
    }
}