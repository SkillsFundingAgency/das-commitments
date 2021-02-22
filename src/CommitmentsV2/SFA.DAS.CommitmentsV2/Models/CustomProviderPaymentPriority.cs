using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public partial class CustomProviderPaymentPriority : Aggregate, ITrackableEntity
    {
        public long EmployerAccountId { get; set; }
        public long ProviderId { get; set; }
        public int PriorityOrder { get; set; }

        public long Id => 0; // There is no Id for the table which has a composite key, but ITrackableEntity requires an implementation

        [JsonIgnore]
        public virtual Account EmployerAccount { get; set; }

        public void UpdateProviderPriority(int priorityOrder, UserInfo userInfo)
        {
            StartTrackingSession(UserAction.UpdateCustomProviderPaymentPriorities, Party.Employer, EmployerAccountId, ProviderId, userInfo);
            ChangeTrackingSession.TrackUpdate(this);
            PriorityOrder = priorityOrder;
            ChangeTrackingSession.CompleteTrackingSession();
        }
    }
}
