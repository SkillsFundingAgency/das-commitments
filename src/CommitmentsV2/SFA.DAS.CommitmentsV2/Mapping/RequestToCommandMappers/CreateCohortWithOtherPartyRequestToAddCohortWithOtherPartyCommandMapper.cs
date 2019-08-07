using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;

namespace SFA.DAS.CommitmentsV2.Mapping.RequestToCommandMappers
{
    public class CreateCohortWithOtherPartyRequestToAddCohortWithOtherPartyCommandMapper : IMapper<CreateCohortWithOtherPartyRequest, AddCohortWithOtherPartyCommand>
    {
        public Task<AddCohortWithOtherPartyCommand> Map(CreateCohortWithOtherPartyRequest source)
        {
            return Task.FromResult(new AddCohortWithOtherPartyCommand
            {
                AccountLegalEntityId = source.AccountLegalEntityId,
                ProviderId = source.ProviderId,
                Message = source.Message,
                UserInfo = source.UserInfo
            });
        }
    }
}