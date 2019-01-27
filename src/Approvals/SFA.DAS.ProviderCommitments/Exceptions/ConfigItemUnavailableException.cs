using System;
using SFA.DAS.ProviderCommitments.Services;

namespace SFA.DAS.ProviderCommitments.Exceptions
{
    public class ConfigItemUnavailableException : ConfigItemException
    {
        public ConfigItemUnavailableException(string requiredConfig, ConfigObjectStatus status, string additionalInformation, DateTime additionalInfomationTime) : base(
            $"The config item '{requiredConfig}' is not available because {status.ToString()}. Additional information:{additionalInformation??"<not available>"} Additional information might have been logged at {additionalInfomationTime} UTC.")
        {
            Status = status;
        }

        public ConfigObjectStatus Status { get; }
    }
}