using System.Threading.Tasks;

namespace SFA.DAS.Commitments.AcademicYearEndProcessor.WebJob.Updater
{
    public interface IAcademicYearEndExpiryProcessor
    {
        Task RunUpdate();

        Task RunChangeOfCircUpdate();
    }
}