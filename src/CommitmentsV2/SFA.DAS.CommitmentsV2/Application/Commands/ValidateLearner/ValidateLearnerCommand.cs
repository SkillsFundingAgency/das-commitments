using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateLearner;

public class ValidateLearnerCommand : IRequest<LearnerValidateApiResponse>
{
    public long ProviderId { get; set; }
    public long LearnerDataId { get; set; }
}