using SFA.DAS.Commitments.Support.SubSite.Extensions;
using SFA.DAS.Commitments.Support.SubSite.Extentions;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.HashingService;
using System;
using System.Collections.Generic;
using System.Linq;
using static SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.GetApprenticeshipsQueryResult;

namespace SFA.DAS.Commitments.Support.SubSite.Mappers
{
    public class ApprenticeshipMapper : IApprenticeshipMapper
    {
        private readonly IHashingService _hashingService;

        public ApprenticeshipMapper(IHashingService hashingService)
        {
            _hashingService = hashingService;
        }

        public UlnSummaryViewModel MapToUlnResultView(GetApprenticeshipsQueryResult response)
        {
            return new UlnSummaryViewModel
            {
                Uln = response.Apprenticeships.First().Uln,
                ApprenticeshipsCount = response.TotalApprenticeships,
                SearchResults = response.Apprenticeships.Select(o => MapToApprenticeshipSearchItemViewModel(o)).OrderBy(a => a.ApprenticeName).ToList()
            };
        }

        public ApprenticeshipViewModel MapToApprenticeshipViewModel(ApprenticeshipDetails apprenticeship)
        {
            var changeRequested = apprenticeship.DataLockPriceTriaged || apprenticeship.DataLockCourseChangeTriaged;

            return new ApprenticeshipViewModel
            {
                FirstName = apprenticeship.FirstName,
                LastName = apprenticeship.LastName,
                Email = apprenticeship.Email ?? "",
                ConfirmationStatusDescription = apprenticeship.ConfirmationStatus?.ToString() ?? "",
                AgreementStatus = apprenticeship.AgreementStatus.GetEnumDescription(),
                PaymentStatus = MapPaymentStatus(apprenticeship.PaymentStatus, apprenticeship.StartDate, apprenticeship.StopDate, apprenticeship.PauseDate),
                Alerts = MapRecordStatus(apprenticeship.UpdateOriginator, apprenticeship.DataLockCourseTriaged, changeRequested),
                Uln = apprenticeship.Uln,
                DateOfBirth = apprenticeship.DateOfBirth,
                CohortReference = apprenticeship.CohortReference,
                EmployerReference = apprenticeship.EmployerRef,
                LegalEntity = apprenticeship.EmployerName,
                TrainingProvider = apprenticeship.ProviderName,
                UKPRN = apprenticeship.ProviderId,
                Trainingcourse = apprenticeship.CourseName,
                ApprenticeshipCode = apprenticeship.co,
                DasTrainingStartDate = apprenticeship.StartDate,
                DasTrainingEndDate = apprenticeship.EndDate,
                TrainingCost = apprenticeship.Cost,
                Version = apprenticeship.TrainingCourseVersionConfirmed ? apprenticeship.TrainingCourseVersion : null,
                Option = apprenticeship.TrainingCourseOption == string.Empty ? "To be confirmed" : apprenticeship.TrainingCourseOption
            };
        }

        public ApprenticeshipSearchItemViewModel MapToApprenticeshipSearchItemViewModel(ApprenticeshipDetails apprenticeship)
        {
            return new ApprenticeshipSearchItemViewModel
            {
                HashedAccountId = _hashingService.HashValue(apprenticeship.AccountLegalEntityId),
                ApprenticeshipHashId = _hashingService.HashValue(apprenticeship.Id),
                ApprenticeName = $"{apprenticeship.FirstName} {apprenticeship.LastName}",
                EmployerName = apprenticeship.EmployerName,
                ProviderUkprn = apprenticeship.ProviderId,
                TrainingDates = $"{apprenticeship.StartDate.ToGdsFormatWithSlashSeperator() ?? "-"} to {apprenticeship.EndDate.ToGdsFormatWithSlashSeperator() ?? "-"}",
                PaymentStatus = MapPaymentStatus(apprenticeship.PaymentStatus, apprenticeship.StartDate, apprenticeship.StopDate, apprenticeship.PauseDate),
                DateOfBirth = apprenticeship.DateOfBirth,
                Uln = apprenticeship.Uln
            };
        }

        private string MapPaymentStatus(PaymentStatus paymentStatus, DateTime? startDate, DateTime? stopDate, DateTime? pauseDate)
        {
            var isStartDateInFuture = startDate.HasValue && startDate.Value > new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            switch (paymentStatus)
            {
                case PaymentStatus.PendingApproval:
                    return "Approval needed";

                case PaymentStatus.Active:
                    return isStartDateInFuture ? "Waiting to start" : "Live";

                case PaymentStatus.Paused:
                    return pauseDate.HasValue ? $"Paused on {pauseDate.ToGdsFormatWithSpaceSeperator()}" : "Paused";

                case PaymentStatus.Withdrawn:
                    return stopDate.HasValue ? $"Stopped on {stopDate.ToGdsFormatWithSpaceSeperator()}" : "Stopped";

                case PaymentStatus.Completed:
                    return "Finished";

                case PaymentStatus.Deleted:
                    return "Deleted";

                default:
                    return string.Empty;
            }
        }

        private IEnumerable<string> MapRecordStatus(Originator? pendingUpdateOriginator, bool dataLockCourseTriaged, bool changeRequested)
        {
            const string changesPending = "Changes pending";
            const string changesForReview = "Changes for review";
            const string changesRequested = "Changes requested";

            var statuses = new List<string>();

            if (pendingUpdateOriginator != null)
            {
                var t = pendingUpdateOriginator == Originator.Employer
                    ? changesPending : changesForReview;
                statuses.Add(t);
            }

            if (dataLockCourseTriaged)
                statuses.Add(changesRequested);

            if (changeRequested)
                statuses.Add(changesForReview);

            return statuses.Distinct();
        }
    }
}