using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class OverlappingTrainingDateRequestDomainService : IOverlappingTrainingDateRequestDomainService
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IAuthenticationService _authenticationService;
        private readonly IOverlapCheckService _overlapCheckService;

        public OverlappingTrainingDateRequestDomainService(Lazy<ProviderCommitmentsDbContext> dbContext, IAuthenticationService authenticationService, IOverlapCheckService overlapCheckService)
        {
            _dbContext = dbContext;
            _authenticationService = authenticationService;
            _overlapCheckService = overlapCheckService;
        }

        public async Task<OverlappingTrainingDateRequest> CreateOverlappingTrainingDateRequest(long apprenticeshipId, long previousApprenticeshipId, UserInfo userInfo, CancellationToken cancellationToken)
        {
            var party = _authenticationService.GetUserParty();
            CheckPartyIsValid(party);

            var draftApprenticeship = await _dbContext.Value.DraftApprenticeships
                .Include(a => a.Cohort)
                .SingleOrDefaultAsync(a => a.Id == apprenticeshipId, cancellationToken);

            if (draftApprenticeship == null) throw new BadRequestException($"Draft Apprenticeship {apprenticeshipId}");
            if (draftApprenticeship.Cohort.IsApprovedByAllParties) throw new InvalidOperationException($"Cohort {draftApprenticeship.Cohort.Id} is approved by all parties and can't be modified");
            if (string.IsNullOrEmpty(draftApprenticeship.Uln) || !draftApprenticeship.StartDate.HasValue || !draftApprenticeship.EndDate.HasValue) throw new InvalidOperationException($"Can't create Overlapping Training Date Request for draft apprenticeship {draftApprenticeship.Id}.  Mandatory data missing");

            var overlapResult =  await _overlapCheckService.CheckForOverlapsOnStartDate(draftApprenticeship.Uln, new Domain.Entities.DateRange(draftApprenticeship.StartDate.Value, draftApprenticeship.EndDate.Value), draftApprenticeship.Id, cancellationToken);

            if (!overlapResult.HasOverlappingStartDate)
            {
                throw new InvalidOperationException($"Can't create Overlapping Training Date Request. Draft apprentiecship {draftApprenticeship.Id} doesn't have overlap with another apprenticeship.");
            }

            var result = draftApprenticeship.CreateOverlappingTrainingDateRequest(party, overlapResult.ApprenticeshipId, userInfo);
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
