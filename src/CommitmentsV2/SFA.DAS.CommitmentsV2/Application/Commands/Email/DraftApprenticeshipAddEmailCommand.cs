namespace SFA.DAS.CommitmentsV2.Application.Commands.Email;

public class DraftApprenticeshipAddEmailCommand : IRequest
{
    public long CohortId { get; set; }
    public long ApprenticeshipId { get; set; }
    public string Email { get; set; }

}
