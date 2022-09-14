using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Domain.Data
{
    public interface IApprenticeshipRepository
    {
        Task<Apprenticeship> GetApprenticeship(long apprenticeshipId);
    }
}