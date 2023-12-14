using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class OverlappingTrainingDateRequestDomainService : IOverlappingTrainingDateRequestDomainService
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IAuthenticationService _authenticationService;
        private readonly IOverlapCheckService _overlapCheckService;
        private readonly ICurrentDateTime _currentDateTime;

        public OverlappingTrainingDateRequestDomainService(Lazy<ProviderCommitmentsDbContext> dbContext, IAuthenticationService authenticationService, IOverlapCheckService overlapCheckService, ICurrentDateTime currentDateTime)
        {
            _dbContext = dbContext;
            _authenticationService = authenticationService;
            _overlapCheckService = overlapCheckService;
            _currentDateTime = currentDateTime;
        }

        public async Task<OverlappingTrainingDateRequest> CreateOverlappingTrainingDateRequest(long apprenticeshipId, Party? originatingParty, long? changeOfEmployerOriginalApprenticeId, UserInfo userInfo, CancellationToken cancellationToken)
        {
            if (originatingParty == null)
            {
                originatingParty = _authenticationService.GetUserParty();
            }
          
            CheckPartyIsValid(originatingParty.Value);

            var draftApprenticeship = await _dbContext.Value.DraftApprenticeships
                .Include(a => a.Cohort)
                .SingleOrDefaultAsync(a => a.Id == apprenticeshipId, cancellationToken);

            if (draftApprenticeship == null) throw new BadRequestException($"Draft Apprenticeship {apprenticeshipId}");
            if (draftApprenticeship.Cohort.IsApprovedByAllParties) throw new InvalidOperationException($"Cohort {draftApprenticeship.Cohort.Id} is approved by all parties and can't be modified");
            if (string.IsNullOrEmpty(draftApprenticeship.Uln) || !draftApprenticeship.StartDate.HasValue || !draftApprenticeship.EndDate.HasValue) throw new InvalidOperationException($"Can't create Overlapping Training Date Request for draft apprenticeship {draftApprenticeship.Id}.  Mandatory data missing");

            var overlapResult =  await _overlapCheckService.CheckForOverlapsOnStartDate(draftApprenticeship.Uln, new Domain.Entities.DateRange(draftApprenticeship.StartDate.Value, draftApprenticeship.EndDate.Value), draftApprenticeship.Id, cancellationToken);

            if (changeOfEmployerOriginalApprenticeId == null && (!overlapResult.HasOverlappingStartDate || overlapResult.ApprenticeshipId == null))
            {
                throw new InvalidOperationException($"Can't create Overlapping Training Date Request. Draft apprenticeship {draftApprenticeship.Id} doesn't have overlap with another apprenticeship.");
            }

            var result = draftApprenticeship.CreateOverlappingTrainingDateRequest(originatingParty.Value,
              changeOfEmployerOriginalApprenticeId ?? overlapResult.ApprenticeshipId.Value, 
                userInfo, _currentDateTime.UtcNow);
            await _dbContext.Value.SaveChangesAsync();
            return result;
        }

        private void CheckPartyIsValid(Party party)
        {
            if (party != Party.Provider )
            {
                throw new DomainException(nameof(party), $"OverlappingTrainingDateRequest is restricted to Providers only - {party} is invalid");
            }
        }
    }
}
