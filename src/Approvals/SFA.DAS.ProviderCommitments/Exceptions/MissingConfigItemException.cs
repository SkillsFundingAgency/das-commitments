namespace SFA.DAS.ProviderCommitments.Exceptions
{
    public class MissingConfigItemException : ConfigItemException
    {
        public MissingConfigItemException(string requiredConfig, string[] availableConfigItems) : base(
            $"The config item '{requiredConfig}' does not exist. The available config items are {string.Join(", ", availableConfigItems)}.")
        {
            // just call base        
        }
    }
}