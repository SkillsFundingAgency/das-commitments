using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models;

public class HiringPayment
{
    public long ApprenticeshipId { get; set; }
    public long? Ukprn { get; set; }
    public string Uln { get; set; }
    public long? EmployerAccountId { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    public HiringPaymentStatus Status { get; set; }

    public virtual Apprenticeship Apprenticeship { get; set; }
}