namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface IEmployerVerificationStatusSyncService
{
    Task SyncPendingEmploymentChecksAsync();
}
