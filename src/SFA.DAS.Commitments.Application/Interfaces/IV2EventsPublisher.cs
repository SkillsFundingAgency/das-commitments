using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.Commitments.Application.Interfaces
{
    /// <summary>
    ///     This represents a service that can publish the V2 events via nservicebus.
    /// </summary>
    public interface IV2EventsPublisher
    {
        Task PublishApprenticeshipDeleted(Commitment commitment, Apprenticeship apprenticeship);
        Task PublishApprenticeshipStopped(Commitment commitment, Apprenticeship apprenticeship);
        Task PublishApprenticeshipStopDateChanged(Commitment commitment, Apprenticeship apprenticeship);
        Task PublishApprenticeshipCreated(IApprenticeshipEvent apprenticeshipEvent);
        Task PublishDataLockTriageApproved(IApprenticeshipEvent apprenticeshipEvent);
        Task PublishApprenticeshipUpdatedApproved(Commitment commitment, Apprenticeship apprenticeship);
        Task PublishApprenticeshipResumed(Commitment commitment, Apprenticeship apprenticeship);
        Task PublishPaymentOrderChanged(long employerAccountId, IEnumerable<int> paymentOrder);
    }
}



