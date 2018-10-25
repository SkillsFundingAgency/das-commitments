using SFA.DAS.Commitments.Domain.Entities.ApprovedApprenticeship;

namespace SFA.DAS.Commitments.Api.Orchestrators.Mappers
{
    public interface IApprovedApprenticeshipMapper
    {
        Types.ApprovedApprenticeship.ApprovedApprenticeship Map(ApprovedApprenticeship approvedApprenticeship);
    }
}