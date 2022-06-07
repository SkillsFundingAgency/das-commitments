using SFA.DAS.Commitments.Support.SubSite.Extensions;
using SFA.DAS.Commitments.Support.SubSite.Extentions;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfProviderChain;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using static SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.GetApprenticeshipsQueryResult;

namespace SFA.DAS.Commitments.Support.SubSite.Mappers
{
    public class ApprenticeshipMapper : IApprenticeshipMapper
    {
        private readonly IEncodingService _encodingService;

        public ApprenticeshipMapper(IEncodingService encodingService)
        {
            _encodingService = encodingService;
        }

        public UlnSummaryViewModel MapToUlnResultView(GetSupportApprenticeshipQueryResult response)
        {
            return new UlnSummaryViewModel
            {
                Uln = response.Apprenticeships.First().Uln,
                ApprenticeshipsCount = response.Apprenticeships.Count,
                SearchResults = response.Apprenticeships.Select(o => MapToApprenticeshipSearchItemViewModel(o)).OrderBy(a => a.ApprenticeName).ToList()
            };
        }

        public ApprenticeshipViewModel MapToApprenticeshipViewModel(GetSupportApprenticeshipQueryResult apprenticeships, GetChangeOfProviderChainQueryResult providerChainQueryResult)
        {
            var apprenticeship = apprenticeships.Apprenticeships.First();

            (string paymentStatusText, string paymentStatusTagColour) = MapPaymentStatus(apprenticeship.PaymentStatus, apprenticeship.StartDate, apprenticeship.StopDate, apprenticeship.PauseDate);

            return new ApprenticeshipViewModel
            {
                FirstName = apprenticeship.FirstName,
                LastName = apprenticeship.LastName,
                Email = apprenticeship.Email ?? "",
                ConfirmationStatusDescription = apprenticeship.ConfirmationStatus?.ToString() ?? "",
                AgreementStatus = apprenticeship.AgreementStatus.GetEnumDescription(),
                PaymentStatus = paymentStatusText,
                Alerts = MapRecordStatus(apprenticeship.Alerts),
                Uln = apprenticeship.Uln,
                DateOfBirth = apprenticeship.DateOfBirth,
                CohortReference = apprenticeship.CohortReference,
                EmployerReference = apprenticeship.EmployerRef,
                LegalEntity = apprenticeship.EmployerName,
                TrainingProvider = apprenticeship.ProviderName,
                UKPRN = apprenticeship.ProviderId,
                Trainingcourse = apprenticeship.CourseName,
                ApprenticeshipCode = apprenticeship.CourseCode,
                DasTrainingStartDate = apprenticeship.StartDate,
                DasTrainingEndDate = apprenticeship.EndDate,
                TrainingCost = apprenticeship.Cost,

                Version = apprenticeship.TrainingCourseVersionConfirmed ? apprenticeship.TrainingCourseVersion : null,
                Option = string.IsNullOrWhiteSpace(apprenticeship.TrainingCourseOption) ? "To be confirmed" : apprenticeship.TrainingCourseOption,

                PauseDate = apprenticeship.PaymentStatus == PaymentStatus.Paused
                ? apprenticeship.PauseDate.ToGdsFormatWithoutDay()
                : string.Empty,

                StopDate = apprenticeship.PaymentStatus == PaymentStatus.Withdrawn
                ? apprenticeship.StopDate.ToGdsFormatWithoutDay()
                : string.Empty,

                CompletionPaymentMonth = apprenticeship.PaymentStatus == PaymentStatus.Completed
                ? apprenticeship.CompletionDate.ToGdsFormatWithoutDay()
                : string.Empty,

                PaymentStatusTagColour = paymentStatusTagColour,

                MadeRedundant = apprenticeship.MadeRedundant,

                ApprenticeshipProviderHistory = MapApprenticeshipProviderHistories(providerChainQueryResult)
            };
        }

        public ApprenticeshipSearchItemViewModel MapToApprenticeshipSearchItemViewModel(SupportApprenticeshipDetails apprenticeship)
        {
            return new ApprenticeshipSearchItemViewModel
            {
                HashedAccountId = _encodingService.Encode(apprenticeship.EmployerAccountId, EncodingType.AccountId),
                ApprenticeshipHashId = _encodingService.Encode(apprenticeship.Id, EncodingType.ApprenticeshipId),
                ApprenticeName = $"{apprenticeship.FirstName} {apprenticeship.LastName}",
                EmployerName = apprenticeship.EmployerName,
                ProviderUkprn = apprenticeship.ProviderId,
                TrainingDates = $"{apprenticeship.StartDate.ToGdsFormatWithSlashSeperator() ?? "-"} to {apprenticeship.EndDate.ToGdsFormatWithSlashSeperator() ?? "-"}",
                PaymentStatus = MapPaymentStatus(apprenticeship.PaymentStatus, apprenticeship.StartDate, apprenticeship.StopDate, apprenticeship.PauseDate).paymentStatusText,
                DateOfBirth = apprenticeship.DateOfBirth,
                Uln = apprenticeship.Uln
            };
        }

        private (string paymentStatusText, string paymentStatusTagColour) MapPaymentStatus(PaymentStatus paymentStatus, DateTime? startDate, DateTime? stopDate, DateTime? pauseDate)
        {
            var isStartDateInFuture = startDate.HasValue && startDate.Value > new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            switch (paymentStatus)
            {
                case PaymentStatus.Active:
                    return isStartDateInFuture ? ("Waiting to start", "") : ("Live", "blue");

                case PaymentStatus.Paused:
                    return ("Paused", "grey");

                case PaymentStatus.Withdrawn:
                    return ("Stopped", "red");

                case PaymentStatus.Completed:
                    return ("Completed", "green");

                default:
                    return (string.Empty, string.Empty);
            }
        }

        private IEnumerable<string> MapRecordStatus(IEnumerable<Alerts> alerts)
        {
            return alerts.Select(o => o.GetEnumDescription()).Distinct().ToList();
        }

        private List<ApprenticeshipProviderHistoryViewModel> MapApprenticeshipProviderHistories(GetChangeOfProviderChainQueryResult providerChainQueryResult)
        {
            if (providerChainQueryResult?.ChangeOfProviderChain == null)
                return new List<ApprenticeshipProviderHistoryViewModel>();

            return providerChainQueryResult.ChangeOfProviderChain.Select(x => new ApprenticeshipProviderHistoryViewModel
            {
                ProviderName = x.ProviderName,
                ApprenticeshipId = x.ApprenticeshipId,
                CreatedOn = x.CreatedOn,
                EndDate = x.EndDate,
                StartDate = x.StartDate,
                StopDate = x.StopDate
            }).ToList();
        }
    }
}