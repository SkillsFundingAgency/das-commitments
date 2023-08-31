using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Mapping.BulkUpload
{
    public class BulkUploadAddDraftApprenticeshipsRequestToCommandMapper : IMapper<BulkUploadAddDraftApprenticeshipsRequest, BulkUploadAddDraftApprenticeshipsCommand>
    {
        public Task<BulkUploadAddDraftApprenticeshipsCommand> Map(BulkUploadAddDraftApprenticeshipsRequest source)
        {
            return Task.FromResult(new BulkUploadAddDraftApprenticeshipsCommand
            {
                BulkUploadDraftApprenticeships = source.BulkUploadDraftApprenticeships.ToList(),
                UserInfo = source.UserInfo,
                ProviderId = source.ProviderId,
                LogId = source.LogId
            });
        }
    }
}
