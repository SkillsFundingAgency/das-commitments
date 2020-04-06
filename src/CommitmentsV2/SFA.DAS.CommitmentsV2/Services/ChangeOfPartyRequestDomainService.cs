using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class ChangeOfPartyRequestDomainService : IChangeOfPartyRequestDomainService
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICurrentDateTime _currentDateTime;

        public ChangeOfPartyRequestDomainService(Lazy<ProviderCommitmentsDbContext> dbContext, IAuthenticationService authenticationService, ICurrentDateTime currentDateTime)
        {
            _dbContext = dbContext;
            _authenticationService = authenticationService;
            _currentDateTime = currentDateTime;
        }

        public async Task<ChangeOfPartyRequest> CreateChangeOfPartyRequest(
            long apprenticeshipId,
            ChangeOfPartyRequestType changeOfPartyRequestType,
            long newPartyId,
            int price,
            DateTime startDate,
            DateTime? endDate,
            UserInfo userInfo,
            CancellationToken cancellationToken)
        {
            var party = _authenticationService.GetUserParty();

            var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(apprenticeshipId, cancellationToken);

            //invariants:
            //if provider, then have permission to do this on behalf of NEW employer

            var result = apprenticeship.CreateChangeOfPartyRequest(changeOfPartyRequestType,
                party,
                newPartyId,
                price,
                startDate,
                endDate,
                userInfo,
                _currentDateTime.UtcNow);

            return result;
        }
    }
}
