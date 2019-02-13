namespace SFA.DAS.ProviderCommitments.Configuration
{
    /// <summary>
    ///     A service that can return configuration from a relevant config repository.
    /// </summary>
    public interface IProviderCommitmentsConfigurationService
    {
        /// <summary>
        ///     A generic configuration provider. Specific methods should be usedin preference.
        /// </summary>
        T GetConfiguration<T>(string serviceName) where T: new();

        /// <summary>
        ///     Returns configuration used by the web security. This is a wrapper around 
        ///     <see cref="GetConfiguration{T}(string)"/>.
        /// </summary>
        /// <returns></returns>
        ProviderCommitmentsSecurityConfiguration GetSecurityConfiguration();
    }
}