using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.AcademicYearEndProcessor.WebJob.Updater
{
    public interface IAcademicYearEndExpiryProcessor
    {
        Task RunDataLock(string s);

        Task RunApprenticeshipUpdateJob(string jobId);
    }
}