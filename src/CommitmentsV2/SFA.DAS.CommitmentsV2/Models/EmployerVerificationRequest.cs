using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models;

public class EmployerVerificationRequest
{
    public long ApprenticeshipId { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    public DateTime? LastCheckedDate { get; set; }
    public EmployerVerificationRequestStatus Status { get; set; }
    public string Notes { get; set; }

    public virtual Apprenticeship Apprenticeship { get; set; }
}
