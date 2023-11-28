using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers
{
    public class GetApprenticeshipResponseMapper : IMapper<GetApprenticeshipQueryResult, GetApprenticeshipResponse>
    {
        public Task<GetApprenticeshipResponse> Map(GetApprenticeshipQueryResult source)
        {
            return Task.FromResult(new GetApprenticeshipResponse
            {
                Id = source.Id,
                CohortId = source.CohortId,
                ProviderId = source.ProviderId,
                ProviderName = source.ProviderName,
                EmployerAccountId = source.EmployerAccountId,
                AccountLegalEntityId = source.AccountLegalEntityId,
                EmployerName = source.EmployerName,
                FirstName = source.FirstName,
                LastName = source.LastName,
                Email = source.Email,
                Uln = source.Uln,
                CourseCode = source.CourseCode,
                StandardUId = source.StandardUId,
                Version = source.Version,
                Option = source.Option,
                CourseName = source.CourseName,
                DeliveryModel = source.DeliveryModel ?? DeliveryModel.Regular,
                StartDate = source.StartDate,
                ActualStartDate = source.ActualStartDate,
                EndDate = source.EndDate,
                DateOfBirth = source.DateOfBirth,
                EmployerReference = source.EmployerReference,
                ProviderReference = source.ProviderReference,
                Status = source.Status,
                StopDate = source.StopDate,
                PauseDate = source.PauseDate,
                CompletionDate = source.CompletionDate,
                EndpointAssessorName = source.EndpointAssessorName,
                HasHadDataLockSuccess = source.HasHadDataLockSuccess,
                ContinuationOfId = source.ContinuationOfId,
                ContinuedById = source.ContinuedById,
                OriginalStartDate = source.OriginalStartDate,
                PreviousProviderId = source.PreviousProviderId,
                PreviousEmployerAccountId = source.PreviousEmployerAccountId,
                ApprenticeshipEmployerTypeOnApproval = source.ApprenticeshipEmployerTypeOnApproval,
                MadeRedundant = source.MadeRedundant,
                ConfirmationStatus = source.ConfirmationStatus,
                EmailAddressConfirmedByApprentice = source.EmailAddressConfirmedByApprentice,
                EmailShouldBePresent = source.EmailShouldBePresent,
                PledgeApplicationId = source.PledgeApplicationId,
                EmploymentEndDate = source.FlexibleEmployment?.EmploymentEndDate,
                EmploymentPrice = source.FlexibleEmployment?.EmploymentPrice,
                RecognisePriorLearning = source.RecognisePriorLearning,
                DurationReducedBy = source.ApprenticeshipPriorLearning?.DurationReducedBy,
                PriceReducedBy = source.ApprenticeshipPriorLearning?.PriceReducedBy,
                TransferSenderId = source.TransferSenderId,
                IsOnFlexiPaymentPilot = source.IsOnFlexiPaymentPilot,
                DurationReducedByHours = source.ApprenticeshipPriorLearning?.DurationReducedByHours,
                TrainingTotalHours = source.TrainingTotalHours,
                IsDurationReducedByRpl = source.ApprenticeshipPriorLearning?.IsDurationReducedByRpl
            });
        }
    }
}
