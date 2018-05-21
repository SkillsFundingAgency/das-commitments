using SFA.DAS.Commitments.Application.Queries.GetApprenticeshipsByUln;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Support.SubSite.Extensions;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.HashingService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;

namespace SFA.DAS.Commitments.Support.SubSite.Orchestrators
{
    public class ApprenticeshipMapper : IApprenticeshipMapper
    {
        private readonly IHashingService _hashingService;

        public ApprenticeshipMapper(IHashingService hashingService)
        {
            _hashingService = hashingService;
        }

        public UlnSearchResultSummaryViewModel MapToUlnResultView(GetApprenticeshipsByUlnResponse response)
        {
            return new UlnSearchResultSummaryViewModel
            {
                Uln = response.Apprenticeships.First().ULN,
                ApprenticeshipsCount = response.TotalCount,
                SearchResults = response.Apprenticeships.Select(o => new UlnSearchResultViewModel
                {
                    HashedAccountId = _hashingService.HashValue(o.EmployerAccountId),
                    ApprenticeshipHashId = _hashingService.HashValue(o.Id),
                    ApprenticeName = $"{o.FirstName} {o.LastName}",
                    EmployerName = o.LegalEntityName,
                    ProviderUkprn = o.ProviderId,
                    TrainingDates = $"{o.StartDate.ToGdsFormatWithSlashSeperator() ?? "-"} to {o.EndDate.ToGdsFormatWithSlashSeperator() ?? "-"}",
                    PaymentStatus = MapPaymentStatus(o.PaymentStatus, o.StartDate, o.StopDate)
                }).ToList()
            };
        }

        public ApprenticeshipViewModel MapToApprenticeshipViewModel(Apprenticeship apprenticeship)
        {
            var changeRequested = apprenticeship.DataLockPriceTriaged || apprenticeship.DataLockCourseChangeTriaged;

            return new ApprenticeshipViewModel
            {
                FirstName = apprenticeship.FirstName,
                LastName = apprenticeship.LastName,
                AgreementStatus = GetEnumDescription(apprenticeship.AgreementStatus),
                PaymentStatus = MapPaymentStatus(apprenticeship.PaymentStatus, apprenticeship.StartDate, apprenticeship.StopDate),
                Alerts = MapRecordStatus(apprenticeship.UpdateOriginator, apprenticeship.DataLockCourseTriaged, changeRequested),
                ULN = apprenticeship.ULN,
                DateOfBirth = apprenticeship.DateOfBirth,
                CohortReference = _hashingService.HashValue(apprenticeship.CommitmentId),
                EmployerReference = apprenticeship.EmployerRef,
                LegalEntity = apprenticeship.LegalEntityName,
                TrainingProvider = apprenticeship.TrainingName,
                UKPRN = apprenticeship.ProviderId,
                Trainingcourse = apprenticeship.TrainingName,
                ApprenticeshipCode = apprenticeship.TrainingCode,

                DasTrainingStartDate = apprenticeship.StartDate,
                DasTrainingEndDate = apprenticeship.StopDate,
                TrainingCost = apprenticeship.Cost
            };
        }

        private string MapPaymentStatus(PaymentStatus paymentStatus, DateTime? startDate, DateTime? stopDate)
        {
            var isStartDateInFuture = startDate.HasValue && startDate.Value > new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            switch (paymentStatus)
            {
                case PaymentStatus.PendingApproval:
                    return "Approval needed";
                case PaymentStatus.Active:
                    return
                        isStartDateInFuture ? "Waiting to start" : "Live";
                case PaymentStatus.Paused:
                    return "Paused";
                case PaymentStatus.Withdrawn:
                    return stopDate.HasValue ? "Stopped" : $"Stopped on {stopDate.ToGdsFormatWithSpaceSeperator()}";
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

        private string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }

        }

    }
}