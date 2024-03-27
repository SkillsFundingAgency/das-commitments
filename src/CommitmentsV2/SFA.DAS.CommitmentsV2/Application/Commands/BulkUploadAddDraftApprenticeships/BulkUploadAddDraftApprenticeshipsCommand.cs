using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships
{
    public class BulkUploadAddDraftApprenticeshipsCommand : IRequest<GetBulkUploadAddDraftApprenticeshipsResponse>
    {
        public List<BulkUploadAddDraftApprenticeshipRequest> BulkUploadDraftApprenticeships { get; set; }
        public long ProviderId { get; set; }
        public long? LogId { get; set; }
        public string ProviderAction { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}
