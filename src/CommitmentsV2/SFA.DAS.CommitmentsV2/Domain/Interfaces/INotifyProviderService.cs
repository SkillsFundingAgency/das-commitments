namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface INotifyProviderService
{
    Task NotifyProvider(long providerId, long apprenticeshipId, string providerName, string template);
}
