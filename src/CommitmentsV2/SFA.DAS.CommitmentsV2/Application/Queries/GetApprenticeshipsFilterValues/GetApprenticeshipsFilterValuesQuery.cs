using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsFilterValues
{
    public class GetApprenticeshipsFilterValuesQuery : IEmployerProviderIdentifier, IRequest<GetApprenticeshipsFilterValuesQueryResult>
    {
        public long? EmployerAccountId { get; set; }
        public long? ProviderId { get; set; }
    }
}
 