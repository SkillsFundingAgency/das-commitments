using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Commitments.Support.SubSite.Application.Queries.GetSupportApprenticeship;
using SFA.DAS.Commitments.Support.SubSite.Extensions;
using SFA.DAS.Commitments.Support.SubSite.Extentions;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfProviderChain;
using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.Commitments.Support.SubSite.Mappers;

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
            SearchResults = response.Apprenticeships.Select(MapToApprenticeshipSearchItemViewModel).OrderBy(a => a.ApprenticeName).ToList()
        };
    }

    public ApprenticeshipViewModel MapToApprenticeshipViewModel(GetSupportApprenticeshipQueryResult apprenticeships, GetChangeOfProviderChainQueryResult providerChainQueryResult)
    {
        var apprenticeship = apprenticeships.Apprenticeships.First();

        var (paymentStatusText, paymentStatusTagColour) = MapPaymentStatus(apprenticeship.PaymentStatus, apprenticeship.StartDate);

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
            Option = apprenticeship.TrainingCourseOption,

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
            DeliveryModel = apprenticeship.DeliveryModel,
            EmploymentPrice = apprenticeship.EmploymentPrice,
            EmploymentEndDate = apprenticeship.EmploymentEndDate,
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
            PaymentStatus = MapPaymentStatus(apprenticeship.PaymentStatus, apprenticeship.StartDate).paymentStatusText,
            DateOfBirth = apprenticeship.DateOfBirth,
            Uln = apprenticeship.Uln
        };
    }

    private static (string paymentStatusText, string paymentStatusTagColour) MapPaymentStatus(PaymentStatus paymentStatus, DateTime? startDate)
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

    private static IEnumerable<string> MapRecordStatus(IEnumerable<Alerts> alerts)
    {
        return alerts.Select(o => o.GetEnumDescription()).Distinct().ToList();
    }

    public ApprenticeshipUpdateViewModel MapToUpdateApprenticeshipViewModel(GetApprenticeshipUpdateQueryResult apprenticeships, SupportApprenticeshipDetails originalApprenticeship)
    {
        var updateCount = apprenticeships?.ApprenticeshipUpdates?.Count;
        if (updateCount == null || updateCount == 0)
        {
            return null;
        }

        if (apprenticeships?.ApprenticeshipUpdates?.Count > 1)
        {
            throw new Exception("Multiple updates found");
        }

        var update = apprenticeships.ApprenticeshipUpdates.First();

        var result = new ApprenticeshipUpdateViewModel
        {
            FirstName = update.FirstName,
            LastName = update.LastName,
            Email = update.Email,
            DateOfBirth = update.DateOfBirth,
            Cost = update.Cost,
            StartDate = update.StartDate,
            EndDate = update.EndDate,
            CourseCode = update.TrainingCode,
            CourseName = update.TrainingName,
            Version = update.TrainingCourseVersion,
            Option = update.TrainingCourseOption,
            DeliveryModel = update.DeliveryModel,
            EmploymentEndDate = update.EmploymentEndDate,
            EmploymentPrice = update.EmploymentPrice,
            Originator = update.Originator,
            CreatedOn = update.CreatedOn,
            OriginalFirstName = originalApprenticeship.FirstName,
            OriginalLastName = originalApprenticeship.LastName
        };

        return result;
    }

    private static List<ApprenticeshipProviderHistoryViewModel> MapApprenticeshipProviderHistories(GetChangeOfProviderChainQueryResult providerChainQueryResult)
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

    public OverlappingTrainingDateRequestViewModel MapToOverlappingTrainingDateRequest(GetOverlappingTrainingDateRequestQueryResult.OverlappingTrainingDateRequest overlappingTrainingDateRequest)
    {
        if (overlappingTrainingDateRequest == null)
        {
            return null;
        }
            
        if (overlappingTrainingDateRequest.Status == OverlappingTrainingDateRequestStatus.Pending)
        {
            return new OverlappingTrainingDateRequestViewModel
            {
                CreatedOn = overlappingTrainingDateRequest.CreatedOn
            };
        }

        return null;
    }
}