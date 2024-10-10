using SFA.DAS.CommitmentsV2.Api.Types.Requests;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateDraftApprenticeshipDetails;

public class ValidateDraftApprenticeshipDetailsCommand : IRequest
{
    public ValidateDraftApprenticeshipRequest DraftApprenticeshipRequest { get; set; }
        
}