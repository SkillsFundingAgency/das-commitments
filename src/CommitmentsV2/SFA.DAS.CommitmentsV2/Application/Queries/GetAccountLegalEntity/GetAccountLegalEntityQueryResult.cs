using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountLegalEntity;

public class GetAccountLegalEntityQueryResult
{
    public long AccountId { get; set; }
    public long MaLegalEntityId { get; set; }
    public string AccountName { get; set; }
    public string LegalEntityName { get; set; }
    public ApprenticeshipEmployerType LevyStatus { get; set; }
}