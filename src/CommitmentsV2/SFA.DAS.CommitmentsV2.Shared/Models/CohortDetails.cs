using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Shared.Models;

public class CohortDetails
{
    public long CohortId { get; set; }
    public string HashedCohortId { get; set; }
    public string LegalEntityName { get; set; }
    public string ProviderName { get; set; }
    public bool IsFundedByTransfer { get; set; }
    public Party WithParty { get; set; }

}