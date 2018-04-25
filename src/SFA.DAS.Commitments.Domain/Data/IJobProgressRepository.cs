using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IJobProgressRepository
    {
        Task<long?> Get_AddEpaToApprenticeships_LastSubmissionEventId();
        Task Set_AddEpaToApprenticeships_LastSubmissionEventId(long lastSubmissionEventsId);
    }
}