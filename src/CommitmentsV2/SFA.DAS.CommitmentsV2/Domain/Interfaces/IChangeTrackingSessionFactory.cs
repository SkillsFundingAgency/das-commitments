using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface IChangeTrackingSessionFactory
{
    IChangeTrackingSession CreateTrackingSession(UserAction userAction, Party party, long employerAccountId, long providerId, UserInfo userInfo);
}