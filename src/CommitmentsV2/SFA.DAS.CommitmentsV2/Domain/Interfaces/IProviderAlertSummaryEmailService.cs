namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IProviderAlertSummaryEmailService
    {
        Task SendAlertSummaryEmails(string jobId);
    }
}
