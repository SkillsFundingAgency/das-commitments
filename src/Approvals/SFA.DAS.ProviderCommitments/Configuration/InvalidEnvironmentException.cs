using System;

namespace SFA.DAS.ProviderCommitments.Configuration
{
    public class InvalidEnvironmentException : Exception
    {
        public InvalidEnvironmentException(string environmentName) : base($"The environment '{environmentName}' is not recognised")
        {
            // just call base        
        }
    }
}