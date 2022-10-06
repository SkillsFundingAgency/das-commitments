using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping.Apprenticeships
{
    public class
        ApprenticeshipToApprenticeshipDetailsMapper : IMapper<Apprenticeship,
            GetApprenticeshipsQueryResult.ApprenticeshipDetails>
    {
        private readonly ICurrentDateTime _currentDateTime;

        public ApprenticeshipToApprenticeshipDetailsMapper(ICurrentDateTime currentDateTime)
        {
            _currentDateTime = currentDateTime;
        }

        public Task<GetApprenticeshipsQueryResult.ApprenticeshipDetails> Map(Apprenticeship source)
        {
            return Task.FromResult(new GetApprenticeshipsQueryResult.ApprenticeshipDetails
            {
                Id = source.Id,
                FirstName = source.FirstName,
                LastName = source.LastName,
                Email = source.Email,
                CourseName = source.CourseName,
                DeliveryModel = source.DeliveryModel ?? Types.DeliveryModel.Regular,
                EmployerName = source.Cohort.AccountLegalEntity.Name,
                ProviderName = source.Cohort.Provider.Name,
                StartDate = source.StartDate.GetValueOrDefault(),
                EndDate = source.EndDate.GetValueOrDefault(),                
                PauseDate = source.PauseDate.GetValueOrDefault(),
                StopDate = source.StopDate,
                EmployerRef = source.EmployerRef,
                ProviderRef = source.ProviderRef,
                CohortReference = source.Cohort.Reference,
                DateOfBirth = source.DateOfBirth.GetValueOrDefault(),
                PaymentStatus = source.PaymentStatus,
                ApprenticeshipStatus = source.MapApprenticeshipStatus(_currentDateTime),
                TotalAgreedPrice = source.PriceHistory.GetPrice(_currentDateTime.UtcNow),
                Uln = source.Uln,
                Alerts = source.MapAlerts(),
                AccountLegalEntityId = source.Cohort.AccountLegalEntityId,
                ProviderId = source.Cohort.ProviderId,
                ConfirmationStatus = source.DisplayConfirmationStatus(
                            source.Email,
                            source.ApprenticeshipConfirmationStatus?.ApprenticeshipConfirmedOn,
                            source.ApprenticeshipConfirmationStatus?.ConfirmationOverdueOn),
                TransferSenderId = source.Cohort.TransferSenderId,
                HasHadDataLockSuccess = source.HasHadDataLockSuccess,
                CourseCode = source.CourseCode,
                Cost = source.Cost,
                PledgeApplicationId = source.Cohort.PledgeApplicationId
            });
        }
    }
}