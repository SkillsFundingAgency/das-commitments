using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.QueryExtensions;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;

public class GetApprenticeshipQueryHandler: IRequestHandler<GetApprenticeshipQuery, GetApprenticeshipQueryResult>
{
    private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

    public GetApprenticeshipQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetApprenticeshipQueryResult> Handle(GetApprenticeshipQuery request, CancellationToken cancellationToken)
    {
        var result = await _dbContext.Value
            .Apprenticeships
            .Include(x => x.FlexibleEmployment)
            .Include(x => x.PriorLearning)
            .GetById(request.ApprenticeshipId, apprenticeship =>
                    new GetApprenticeshipQueryResult
                    {
                        Id = apprenticeship.Id,
                        CohortId = apprenticeship.CommitmentId,
                        CourseCode = apprenticeship.CourseCode,
                        StandardUId = apprenticeship.StandardUId,
                        Version = apprenticeship.TrainingCourseVersion,
                        Option = apprenticeship.TrainingCourseOption,
                        CourseName = apprenticeship.CourseName,
                        DeliveryModel = apprenticeship.DeliveryModel,
                        EmployerAccountId = apprenticeship.Cohort.EmployerAccountId,
                        AccountLegalEntityId = apprenticeship.Cohort.AccountLegalEntityId,
                        EmployerName = apprenticeship.Cohort.AccountLegalEntity.Name,
                        ProviderId = apprenticeship.Cohort.ProviderId,
                        ProviderName = apprenticeship.Cohort.Provider.Name,
                        DateOfBirth = apprenticeship.DateOfBirth.Value,
                        FirstName = apprenticeship.FirstName,
                        LastName = apprenticeship.LastName,
                        Email = apprenticeship.Email,
                        Uln = apprenticeship.Uln,
                        StartDate = apprenticeship.StartDate,
                        ActualStartDate = apprenticeship.ActualStartDate,
                        EndDate = apprenticeship.EndDate.Value,
                        EndpointAssessorName = apprenticeship.EpaOrg.Name,
                        EmployerReference = apprenticeship.EmployerRef,
                        ProviderReference = apprenticeship.ProviderRef,
                        Status = apprenticeship.GetApprenticeshipStatus(null),
                        StopDate = apprenticeship.StopDate,
                        PauseDate = apprenticeship.PauseDate,
                        HasHadDataLockSuccess = apprenticeship.HasHadDataLockSuccess,
                        CompletionDate = apprenticeship.CompletionDate,
                        ContinuationOfId = apprenticeship.ContinuationOfId,
                        ContinuedById = apprenticeship.Continuation != null ? apprenticeship.Continuation.Id : default(long?),
                        PreviousProviderId = apprenticeship.IsContinuation
                            ? apprenticeship.PreviousApprenticeship.Cohort.ProviderId
                            : default(long?),
                        PreviousEmployerAccountId = apprenticeship.IsContinuation
                            ? apprenticeship.PreviousApprenticeship.Cohort.EmployerAccountId
                            : default(long?),
                        OriginalStartDate = apprenticeship.OriginalStartDate,
                        ApprenticeshipEmployerTypeOnApproval = apprenticeship.Cohort.ApprenticeshipEmployerTypeOnApproval,
                        MadeRedundant = apprenticeship.MadeRedundant,
                        EmailAddressConfirmedByApprentice = apprenticeship.EmailAddressConfirmed == true,
                        EmailShouldBePresent = apprenticeship.Cohort.EmployerAndProviderApprovedOn >= new DateTime(2021,9,10) && apprenticeship.ContinuationOfId == null,
                        ConfirmationStatus = Models.Apprenticeship.DisplayConfirmationStatus(
                            apprenticeship.Email,
                            apprenticeship.ApprenticeshipConfirmationStatus != null ? apprenticeship.ApprenticeshipConfirmationStatus.ApprenticeshipConfirmedOn : null,
                            apprenticeship.ApprenticeshipConfirmationStatus != null ? apprenticeship.ApprenticeshipConfirmationStatus.ConfirmationOverdueOn : null),
                        PledgeApplicationId = apprenticeship.Cohort.PledgeApplicationId,
                        FlexibleEmployment = apprenticeship.FlexibleEmployment,
                        RecognisePriorLearning = apprenticeship.RecognisePriorLearning,
                        ApprenticeshipPriorLearning = apprenticeship.PriorLearning,
                        TransferSenderId = apprenticeship.Cohort.TransferSenderId,
                        IsOnFlexiPaymentPilot = apprenticeship.IsOnFlexiPaymentPilot,
                        TrainingTotalHours = apprenticeship.TrainingTotalHours,
                        EmployerHasEditedCost = apprenticeship.EmployerHasEditedCost
                    },
                cancellationToken);

        return result;
    }
}