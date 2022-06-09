using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Mapping.Apprenticeships
{
    public class ApprenticeshipToSupportApprenticeshipDetailsMapperMapper : IMapper<Apprenticeship, SupportApprenticeshipDetails>
    {
        private readonly ICurrentDateTime _currentDateTime;

        public ApprenticeshipToSupportApprenticeshipDetailsMapperMapper(ICurrentDateTime currentDateTime)
        {
            _currentDateTime = currentDateTime;
        }

        public Task<SupportApprenticeshipDetails> Map(Apprenticeship source)
        {
            return Task.FromResult(new SupportApprenticeshipDetails
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
                EmployerAccountId = source.Cohort.EmployerAccountId,
                ConfirmationStatus = source.DisplayConfirmationStatus(
                            source.Email,
                            source.ApprenticeshipConfirmationStatus?.ApprenticeshipConfirmedOn,
                            source.ApprenticeshipConfirmationStatus?.ConfirmationOverdueOn),
                TransferSenderId = source.Cohort.TransferSenderId,
                HasHadDataLockSuccess = source.HasHadDataLockSuccess,
                CourseCode = source.CourseCode,
                Cost = source.Cost,
                StopDate = source.StopDate,
                CompletionDate = source.CompletionDate,
                MadeRedundant = source.MadeRedundant,
                AgreementStatus = GetAgreementStatus(source.Cohort.Approvals),
                StandardUId = source.StandardUId,
                TrainingCourseVersionConfirmed = source.TrainingCourseVersionConfirmed,
                TrainingCourseOption = source.TrainingCourseOption,
                EmploymentPrice = source.FlexibleEmployment?.EmploymentPrice,
                EmploymentEndDate = source.FlexibleEmployment?.EmploymentEndDate,

            });
        }

        private static AgreementStatus GetAgreementStatus(Party party)
        {
            if (party == Party.None)
                return AgreementStatus.NotAgreed;

            if (party == Party.Employer)
                return AgreementStatus.EmployerAgreed;

            if (party == Party.Provider)
                return AgreementStatus.ProviderAgreed;

            if (party.HasFlag(Party.Provider) && party.HasFlag(Party.Employer))
                return AgreementStatus.BothAgreed;

            return AgreementStatus.NotAgreed;
        }
    }
}