using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateUln;

public class ValidateUlnOverlapCommand : IRequest<ValidateUlnOverlapResult>
{
    public long? ApprenticeshipId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string ULN { get; set; }
}