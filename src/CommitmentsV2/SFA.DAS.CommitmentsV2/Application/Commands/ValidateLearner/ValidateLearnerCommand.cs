using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateLearner;

public class ValidateLearnerCommand : IRequest<LearnerValidateApiResponse>
{
    public long ProviderId { get; set; }
    public long LearnerDataId { get; set; }
    public LearnerData LearnerData { get; set; }
    public ProviderStandardResults ProviderStandardsData { get; set; }
    public int? OtjTrainingHours { get; set; }
}