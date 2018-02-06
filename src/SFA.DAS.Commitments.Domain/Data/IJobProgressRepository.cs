using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IJobProgressRepository
    {
        Task<long?> Get_AddEpaToApprenticeships_LastSubmissionEventIdAsync();
        Task Set_AddEpaToApprenticeships_LastSubmissionEventIdAsync(long lastSubmissionEventsId);
    }
}
