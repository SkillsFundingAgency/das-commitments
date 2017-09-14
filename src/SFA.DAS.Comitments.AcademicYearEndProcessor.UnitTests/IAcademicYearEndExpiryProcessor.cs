using System.Threading.Tasks;

namespace SFA.DAS.Comitments.AcademicYearEndProcessor.UnitTests
{
    public interface IAcademicYearEndExpiryProcessor
    {
        Task RunUpdate();
    }
}