using System;

namespace SFA.DAS.ProviderCommitments.Interfaces
{
    /// <summary>
    ///     Responsible for application specific types.
    /// </summary>
    public interface IAssemblyDiscoveryService
    {
        /// <summary>
        ///     Returns all application specific types with the specified
        ///     class name (i.e. {any-name-space}.{class-name}.
        /// </summary>
        Type[] GetApplicationTypes(string matchingClassName);
    }
}