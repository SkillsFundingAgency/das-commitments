namespace SFA.DAS.Commitments.Application.Services
{
    public enum EnvironmentType
    {
        /// <summary>
        ///     The current environment is unknown
        /// </summary>
        Unknown,

        /// <summary>
        ///     A development-like environment
        /// </summary>
        Development,

        /// <summary>
        ///     A test-like environment
        /// </summary>
        Test,

        /// <summary>
        ///     A production-like environment
        /// </summary>
        Production
    }
}