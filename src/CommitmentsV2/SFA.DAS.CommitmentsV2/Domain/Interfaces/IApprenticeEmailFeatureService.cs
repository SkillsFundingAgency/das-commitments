using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IApprenticeEmailFeatureService
    {
        bool IsEnabled { get; }
        Task<bool> ApprenticeEmailIsRequiredFor(long employerAccountId, long providerId);
    }
}