using SFA.DAS.Commitments.Application.Configuration;
using SFA.DAS.Commitments.Application.Services;

namespace SFA.DAS.Commitments.Application.Interfaces
{
    /// <summary>
    ///     Represents a service that provide information about the type of hosting environment the app is currently
    ///     running in.
    /// </summary>
    public interface IHostingEnvironment
    {
        /// <summary>
        ///     The specific environment (dev, test, test2 etc)
        /// </summary>
        Environment Environment { get; }

        /// <summary>
        ///     The class of environment (dev, test, prod)
        /// </summary>
        EnvironmentType EnvironmentType { get; }

        /// <summary>
        ///     Indicates if running in a dev-like environment
        /// </summary>
        bool IsDevelopment {get; }

        /// <summary>
        ///     Indicates if running in a test-like environment
        /// </summary>
        bool IsTest { get; }

        /// <summary>
        ///     Indicates if running in a prod-like environment
        /// </summary>
        bool IsProduction { get; }
    }
}