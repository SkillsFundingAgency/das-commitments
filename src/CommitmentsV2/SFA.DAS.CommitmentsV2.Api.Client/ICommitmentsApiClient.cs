using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.Client
{
    public interface ICommitmentsApiClient
    {
        Task<bool> HealthCheck();
    }
}
