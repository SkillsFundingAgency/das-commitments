using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface ITrainingProgrammeRepository
    {
        Task<List<Standard>> GetAllStandards();
        Task<List<Framework>> GetAllFrameworks();
        Task<List<StandardVersion>> GetAllStandardVersions();
    }
}