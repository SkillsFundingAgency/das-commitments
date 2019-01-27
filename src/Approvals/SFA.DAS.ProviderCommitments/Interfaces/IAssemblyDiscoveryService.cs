using System;

namespace SFA.DAS.ProviderCommitments.Interfaces
{
    /// <summary>
    ///     Responsible for discovering application specific types.
    /// </summary>
    public interface IAssemblyDiscoveryService
    {
        /// <summary>
        ///     Returns all application specific types with the specified
        ///     class name (with or without name-space [{any-name-space}.]{class-name}.
        /// </summary>
        Type[] GetApplicationTypes(string matchingClassName);
    }
}