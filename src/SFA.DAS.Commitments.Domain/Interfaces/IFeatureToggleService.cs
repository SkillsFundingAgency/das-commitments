namespace SFA.DAS.Commitments.Domain.Interfaces
{
    public interface IFeatureToggleService
    {
        bool IsEnabled(string featureToggleName);
    }
}