using Microsoft.Data.SqlClient;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Data.Extensions
{
    public static class CommitmentsDbContextExtensions
    {
        public static Task ProcessFullyApprovedCohort(this ProviderCommitmentsDbContext db, long cohortId, long accountId, ApprenticeshipEmployerType apprenticeshipEmployerType)
        {
            return db.ExecuteSqlCommandAsync(
                "EXEC ProcessFullyApprovedCohort @cohortId, @accountId, @apprenticeshipEmployerType",
                new SqlParameter("cohortId", cohortId),
                new SqlParameter("accountId", accountId),
                new SqlParameter("apprenticeshipEmployerType", apprenticeshipEmployerType));
        }

        public static async Task<Cohort> GetCohortAggregate(this ProviderCommitmentsDbContext db, long cohortId, CancellationToken cancellationToken)
        {
            var cohort = await db.Cohorts
                .Include(c => c.Apprenticeships).ThenInclude(a => a.FlexibleEmployment)
                .Include(c => c.Apprenticeships).ThenInclude(a => a.PriorLearning)
                .Include(c => c.TransferRequests)
                .SingleOrDefaultAsync(c => c.Id == cohortId, cancellationToken);
            if (cohort == null) throw new BadRequestException($"Cohort {cohortId} was not found");
            if (cohort.IsApprovedByAllParties) throw new CohortAlreadyApprovedException($"Cohort {cohortId} is approved by all parties and can't be modified");
            return cohort;
        }

        public static async Task<DraftApprenticeship> GetDraftApprenticeshipAggregate(this ProviderCommitmentsDbContext db, long cohortId, long apprenticeshipId, CancellationToken cancellationToken)
        {
            var draftApprenticeship = await db.DraftApprenticeships
                .Include(a => a.Cohort)
                .Include(a => a.FlexibleEmployment)
                .Include(a => a.PriorLearning)
                .SingleOrDefaultAsync(a => a.Id == apprenticeshipId && a.CommitmentId == cohortId, cancellationToken);
            if (draftApprenticeship == null) throw new BadRequestException($"Draft Apprenticeship {apprenticeshipId}  in Cohort {cohortId} was not found");
            if (draftApprenticeship.Cohort.IsApprovedByAllParties) throw new CohortAlreadyApprovedException($"Cohort {cohortId} is approved by all parties and can't be modified");
            return draftApprenticeship;
        }

        public static async Task<Apprenticeship> GetApprenticeshipAggregate(this ProviderCommitmentsDbContext db, long apprenticeshipId, CancellationToken cancellationToken)
        {
            var apprenticeship = await db.Apprenticeships
                .Include(a => a.Cohort).ThenInclude(c => c.AccountLegalEntity)
                .Include(a => a.DataLockStatus)
                .Include(a => a.PriceHistory)
                .Include(a => a.ApprenticeshipUpdate)
                .Include(a => a.ChangeOfPartyRequests)
                .Include(a => a.ApprenticeshipConfirmationStatus)
                .Include(a => a.PriorLearning)
                .Include(a => a.FlexibleEmployment)
                .SingleOrDefaultAsync(a => a.Id == apprenticeshipId, cancellationToken);

            if (apprenticeship == null) throw new BadRequestException($"Apprenticeship {apprenticeshipId} was not found");

            return apprenticeship;
        }

        public static async Task<ChangeOfPartyRequest> GetChangeOfPartyRequestAggregate(this ProviderCommitmentsDbContext db, long changeOfPartyId, CancellationToken cancellationToken)
        {
            var result = await db.ChangeOfPartyRequests
                .Include(r => r.AccountLegalEntity)
                .Include(r => r.Cohort)
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(c => c.Id == changeOfPartyId, cancellationToken);
            if (result == null) throw new BadRequestException($"ChangeOfPartyRequest {changeOfPartyId} was not found");
            return result;
        }

        public static async Task<OverlappingTrainingDateRequest> GetOverlappingTrainingDateRequestAggregate(this ProviderCommitmentsDbContext db, long previousApprenticeshipId, CancellationToken cancellationToken)
        {
            var result = await db.OverlappingTrainingDateRequests
                .Include(r => r.DraftApprenticeship)
                .Include(r => r.PreviousApprenticeship)
                .SingleOrDefaultAsync(c => c.PreviousApprenticeshipId == previousApprenticeshipId
                && c.Status == OverlappingTrainingDateRequestStatus.Pending, cancellationToken);
            return result;
        }

        public static async Task<DraftApprenticeship> GetOLTDResolvedDraftApprenticeshipAggregate(this ProviderCommitmentsDbContext db, long cohortId, long apprenticeshipId, CancellationToken cancellationToken)
        {
            var draftApprenticeship = await db.DraftApprenticeships
                .Include(a => a.Cohort).ThenInclude(c => c.Provider)
                .SingleOrDefaultAsync(a => a.Id == apprenticeshipId && a.CommitmentId == cohortId, cancellationToken);
            if (draftApprenticeship == null) throw new BadRequestException($"Draft Apprenticeship {apprenticeshipId}  in Cohort {cohortId} was not found");
            if (draftApprenticeship.Cohort.IsApprovedByAllParties) throw new InvalidOperationException($"Cohort {cohortId} is approved by all parties and can't be modified");
            return draftApprenticeship;
        }
    }
}