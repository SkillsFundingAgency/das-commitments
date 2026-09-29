namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface INotifyProviderService
{
    Task NotifyProvider(long providerId, string apprenticeshipHashedId, string template, string employerName = null);
}