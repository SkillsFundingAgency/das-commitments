using SFA.DAS.Commitments.Application.Queries.GetApprenticeshipsByUln;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Support.SubSite.Extensions;
using SFA.DAS.Commitments.Support.SubSite.Extentions;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.HashingService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Commitments.Support.SubSite.Mappers
{
    public class ApprenticeshipMapper : IApprenticeshipMapper
    {
        private readonly IHashingService _hashingService;

        public ApprenticeshipMapper(IHashingService hashingService)
        {
            _hashingService = hashingService;
        }

        public UlnSummaryViewModel MapToUlnResultView(GetApprenticeshipsByUlnResponse response)
        {
            return new UlnSummaryViewModel
            {
                Uln = response.Apprenticeships.First().ULN,
                ApprenticeshipsCount = response.TotalCount,
                SearchResults = response.Apprenticeships.Select(o => MapToApprenticeshipSearchItemViewModel(o)).OrderBy(a => a.ApprenticeName).ToList()
            };
        }

        public ApprenticeshipViewModel MapToApprenticeshipViewModel(Apprenticeship apprenticeship)
        {
            var changeRequested = apprenticeship.DataLockPriceTriaged || apprenticeship.DataLockCourseChangeTriaged;

            return new ApprenticeshipViewModel
            {
                FirstName = apprenticeship.FirstName,
                LastName = apprenticeship.LastName,
                AgreementStatus = apprenticeship.AgreementStatus.GetEnumDescription(),
                PaymentStatus = MapPaymentStatus(apprenticeship.PaymentStatus, apprenticeship.StartDate, apprenticeship.StopDate, apprenticeship.PauseDate),
                Alerts = MapRecordStatus(apprenticeship.UpdateOriginator, apprenticeship.DataLockCourseTriaged, changeRequested),
                Uln = apprenticeship.ULN,
                DateOfBirth = apprenticeship.DateOfBirth,
                CohortReference = _hashingService.HashValue(apprenticeship.CommitmentId),
                EmployerReference = apprenticeship.EmployerRef,
                LegalEntity = apprenticeship.LegalEntityName,
                TrainingProvider = apprenticeship.ProviderName,
                UKPRN = apprenticeship.ProviderId,
                Trainingcourse = apprenticeship.TrainingName,
                ApprenticeshipCode = apprenticeship.TrainingCode,
                DasTrainingStartDate = apprenticeship.StartDate,
                DasTrainingEndDate = apprenticeship.EndDate,
                TrainingCost = apprenticeship.Cost,
                Version = apprenticeship.TrainingCourseVersionConfirmed ? apprenticeship.TrainingCourseVersion : null,
                Option = apprenticeship.TrainingCourseOption == string.Empty ? "To be confirmed" : apprenticeship.TrainingCourseOption
            };
        }

        public ApprenticeshipSearchItemViewModel MapToApprenticeshipSearchItemViewModel(Apprenticeship apprenticeship)
        {
            return new ApprenticeshipSearchItemViewModel
            {
                HashedAccountId = _hashingService.HashValue(apprenticeship.EmployerAccountId),
                ApprenticeshipHashId = _hashingService.HashValue(apprenticeship.Id),
                ApprenticeName = $"{apprenticeship.FirstName} {apprenticeship.LastName}",
                EmployerName = apprenticeship.LegalEntityName,
                ProviderUkprn = apprenticeship.ProviderId,
                TrainingDates = $"{apprenticeship.StartDate.ToGdsFormatWithSlashSeperator() ?? "-"} to {apprenticeship.EndDate.ToGdsFormatWithSlashSeperator() ?? "-"}",
                PaymentStatus = MapPaymentStatus(apprenticeship.PaymentStatus, apprenticeship.StartDate, apprenticeship.StopDate, apprenticeship.PauseDate),
                DateOfBirth = apprenticeship.DateOfBirth,
                Uln = apprenticeship.ULN
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