using System;

namespace SFA.DAS.ProviderCommitments.Interfaces
{
    /// <summary>
    ///     A service that can return configuration from a relevant config repository.
    /// </summary>
    /// <remarks>
    ///     This config provider does not provide the whole config section. Instead,
    ///     it will look for a property on the document root with the same name as the
    ///     config type requested and de-serialise just that property. This allows more
    ///     specific config to be provided, as it will just be a section.
    /// </remarks>
    public interface IProviderCommitmentsConfigurationService
    {
        /// <summary>
        ///     A generic configuration provider. Specific methods such as <see cref="GetSecurityConfiguration"/>
        ///     should be created and used in preference.
        /// </summary>
        T Get<T>() where T: class, new();

        /// <summary>
        ///     Similar to the generic Get but with type supplied as parameter.
        /// </summary>
        /// <param name="requiredConfigType"></param>
        /// <returns></returns>
        object Get(Type requiredConfigType);

        /// <summary>
        ///     Returns configuration used by the web security. This is a wrapper around <see cref="Get{T}"/>.
        /// </summary>
        /// <returns></returns>
        ProviderCommitmentsSecurityConfiguration GetSecurityConfiguration();
    }
}