using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Types;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships
{
    public class BulkUploadAddDraftApprenticeshipsCommand : IRequest
    {
        public List<BulkUploadAddDraftApprenticeshipRequest> DraftApprenticeships { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}
