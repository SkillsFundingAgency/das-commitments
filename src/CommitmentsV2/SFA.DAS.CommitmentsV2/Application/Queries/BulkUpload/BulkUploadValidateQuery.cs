using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.BulkUpload
{
    public class BulkUploadValidateQuery : IRequest<BulkUploadResponse>
    {
        public BulkUploadValidatorRequest request { get; set; }

        public UserInfo UserInfo { get; set; }
    }
}