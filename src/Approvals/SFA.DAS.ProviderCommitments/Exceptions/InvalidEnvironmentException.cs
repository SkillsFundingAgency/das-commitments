using System;

namespace SFA.DAS.ProviderCommitments.Exceptions
{
    public class InvalidEnvironmentException : Exception
    {
        public InvalidEnvironmentException(string environmentName) : base($"The environment '{environmentName}' is not recognised")
        {
            // just call base        
        }
    }
}