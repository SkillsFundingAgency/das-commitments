using System.Threading.Tasks;

namespace SFA.DAS.Comitments.AcademicYearEndProcessor.WebJob.Updater
{
    public interface IAcademicYearEndExpiryProcessor
    {
        Task RunUpdate();
    }
}