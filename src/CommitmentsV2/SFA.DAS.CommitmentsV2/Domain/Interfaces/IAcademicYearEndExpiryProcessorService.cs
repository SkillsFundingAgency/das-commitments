namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IAcademicYearEndExpiryProcessorService
    {
        Task ExpireDataLocks(string jobId);

        Task ExpireApprenticeshipUpdates(string jobId);
    }
}