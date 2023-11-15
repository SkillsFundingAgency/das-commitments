using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddAndApproveDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Mapping.BulkUpload
{
    public class BulkUploadAddAndApproveDraftApprenticeshipsRequestToCommandMapper : IMapper<BulkUploadAddAndApproveDraftApprenticeshipsRequest, BulkUploadAddAndApproveDraftApprenticeshipsCommand>
    {
        public Task<BulkUploadAddAndApproveDraftApprenticeshipsCommand> Map(BulkUploadAddAndApproveDraftApprenticeshipsRequest source)
        {
            return Task.FromResult(new BulkUploadAddAndApproveDraftApprenticeshipsCommand
            {
                BulkUploadDraftApprenticeships = source.BulkUploadAddAndApproveDraftApprenticeships.ToList(),
                UserInfo = source.UserInfo,
                ProviderId = source.ProviderId,
                LogId = source.LogId
            });
        }
    }
}
