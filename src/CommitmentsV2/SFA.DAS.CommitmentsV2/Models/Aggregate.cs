using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models;

public abstract class Aggregate : Entity
{
    protected IChangeTrackingSession ChangeTrackingSession { get; private set; }

    protected void StartTrackingSession(UserAction userAction, Party party, long employerAccountId, long providerId, UserInfo userInfo, long? apprenticeshipId = default(long?))
    {
        ChangeTrackingSession = new ChangeTrackingSession(new StateService(), userAction, party, employerAccountId, providerId, userInfo, apprenticeshipId);
    }
}