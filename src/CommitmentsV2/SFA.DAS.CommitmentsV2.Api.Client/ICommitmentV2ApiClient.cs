using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.Client
{
    public interface ICommitmentV2ApiClient
    {
        Task<bool> HealthCheck();
    }
}
