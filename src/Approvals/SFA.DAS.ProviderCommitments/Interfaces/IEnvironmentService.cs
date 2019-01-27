using SFA.DAS.ProviderCommitments.Services;

namespace SFA.DAS.ProviderCommitments.Configuration
{

    /// <summary>
    ///     A service that can provide information about the current environment (test, dev etc).
    /// </summary>
    public interface IEnvironmentService
    {
        EnvironmentType EnvironmentType { get; }
        string EnvironmentName { get; }
    }
}