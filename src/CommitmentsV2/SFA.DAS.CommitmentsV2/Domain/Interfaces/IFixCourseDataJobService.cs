using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IFixCourseDataJobService
    {
        Task RunUpdate();
    }
}