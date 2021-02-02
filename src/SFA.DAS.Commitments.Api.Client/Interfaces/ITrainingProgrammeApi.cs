using System.Threading.Tasks;
using SFA.DAS.Commitments.Api.Types.TrainingProgramme;

namespace SFA.DAS.Commitments.Api.Client.Interfaces
{
    public interface ITrainingProgrammeApi
    {
        Task<GetAllTrainingProgrammeStandardsResponse> GetAllStandards();
        Task<GetAllTrainingProgrammesResponse> GetAll();
        Task<GetTrainingProgrammeResponse> Get(string id);
    }
}