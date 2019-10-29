using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class ChangeTrackingSessionFactory : IChangeTrackingSessionFactory
    {
        private readonly IStateService _stateService;

        public ChangeTrackingSessionFactory(IStateService stateService)
        {
            _stateService = stateService;
        }

        public IChangeTrackingSession CreateTrackingSession(UserAction userAction, Party party, long employerAccountId,
            long providerId, UserInfo userInfo)
        {
            return new ChangeTrackingSession(_stateService, userAction, party, employerAccountId, providerId, userInfo);
        }
    }
}
