using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipApproval;

public class GetApprenticeshipApprovalQueryResult
{
    public long ApprenticeshipId { get; set; }
    public ApprenticeshipStatus ApprenticeshipStatus { get; set; }
    public Guid ApprovalRequestId { get; set; }
    public CocApprovalResultStatus? ApprovalRequestStatus { get; set; }
    public virtual ICollection<ChangeItem> Items { get; set; }
    public string Name { get; set; }
    public string ULN { get; set; }
    public DateTime? StartDate { get; set; }
    public string CourseCode { get; set; }
    public string CourseName { get; set; }
    public string ProviderName { get; set; }
    public long UKPRN { get; set; }
    public string AccountLegalEntityName { get; set; }
    public long AccountLegalEntityId { get; set; }
    public long AccountId { get; set; }

    public class ChangeItem
    {
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime? EffectiveFromDate { get; set; }
    }
}