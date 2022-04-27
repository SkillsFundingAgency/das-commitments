using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Types;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddAndApproveDraftApprenticeships
{
    public class BulkUploadAddAndApproveDraftApprenticeshipsCommand : IRequest<BulkUploadAddAndApproveDraftApprenticeshipsResponse>
    {
        public List<BulkUploadAddDraftApprenticeshipRequest> BulkUploadDraftApprenticeships { get; set; }
        public long ProviderId { get; set; }
        public UserInfo UserInfo { get; set; }
        public BulkReservationValidationResults BulkReservationValidationResults { get; set; }
    }
}
