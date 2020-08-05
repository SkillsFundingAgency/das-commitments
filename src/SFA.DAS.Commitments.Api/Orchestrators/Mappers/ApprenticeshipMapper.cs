using System.Collections.Generic;
using System.Linq;

using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Extensions;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship.Apprenticeship;
using PriceHistory = SFA.DAS.Commitments.Api.Types.Apprenticeship.PriceHistory;


namespace SFA.DAS.Commitments.Api.Orchestrators.Mappers
{
    public class ApprenticeshipMapper : IApprenticeshipMapper
    {
        public Apprenticeship MapFrom(Domain.Entities.Apprenticeship source, CallerType callerType)
        {
            return new Apprenticeship
            {
                Id = source.Id,
                CommitmentId = source.CommitmentId,
                EmployerAccountId = source.EmployerAccountId,
                ProviderId = source.ProviderId,
                TransferSenderId = source.TransferSenderId,
                Reference = source.Reference,
                FirstName = source.FirstName,
                LastName = source.LastName,
                ULN = source.ULN,
                TrainingType = (Api.Types.Apprenticeship.Types.TrainingType)source.TrainingType,
                TrainingCode = source.TrainingCode,
                TrainingName = source.TrainingName,
                Cost = source.Cost,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                PauseDate = source.PauseDate,
                CompletionDate = source.CompletionDate,
                StopDate = source.StopDate,
                PaymentStatus = (Api.Types.Apprenticeship.Types.PaymentStatus)source.PaymentStatus,
                AgreementStatus = (Api.Types.AgreementStatus)source.AgreementStatus,
                DateOfBirth = source.DateOfBirth,
                NINumber = source.NINumber,
                EmployerRef = source.EmployerRef,
                ProviderRef = source.ProviderRef,
                CanBeApproved = callerType == CallerType.Employer
                        ? source.EmployerCanApproveApprenticeship
                        : source.ProviderCanApproveApprenticeship,
                PendingUpdateOriginator = (Api.Types.Apprenticeship.Types.Originator?)source.UpdateOriginator,
                ProviderName = source.ProviderName,
                LegalEntityId = source.LegalEntityId,
                LegalEntityName = source.LegalEntityName,
                AccountLegalEntityPublicHashedId = source.AccountLegalEntityPublicHashedId,
                DataLockCourse = source.DataLockCourse,
                DataLockPrice = source.DataLockPrice,
                DataLockCourseTriaged = source.DataLockCourseTriaged,
                DataLockCourseChangeTriaged = source.DataLockCourseChangeTriaged,
                DataLockPriceTriaged = source.DataLockPriceTriaged,
                HasHadDataLockSuccess = source.HasHadDataLockSuccess,
                EndpointAssessorName = source.EndpointAssessorName,
                ReservationId = source.ReservationId,
                ContinuationOfId = source.ContinuationOfId,
                MadeRedundant = source.MadeRedundant
            };
        }

        public Apprenticeship MapFromV2(Domain.Entities.Apprenticeship source, CallerType callerType)
        {
            return new Apprenticeship
            {
                Id = source.Id,
                CommitmentId = source.CommitmentId,
                EmployerAccountId = source.EmployerAccountId,
                ProviderId = source.ProviderId,
                TransferSenderId = source.TransferSenderId,
                Reference = source.Reference,
                FirstName = source.FirstName,
                LastName = source.LastName,
                ULN = source.ULN,
                TrainingType = (Api.Types.Apprenticeship.Types.TrainingType)source.TrainingType,
                TrainingCode = source.TrainingCode,
                TrainingName = source.TrainingName,
                Cost = source.Cost,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                PauseDate = source.PauseDate,
                StopDate = source.StopDate,
                PaymentStatus = (Api.Types.Apprenticeship.Types.PaymentStatus)source.PaymentStatus,
                AgreementStatus = (Api.Types.AgreementStatus)source.AgreementStatus,
                DateOfBirth = source.DateOfBirth,
                NINumber = source.NINumber,
                EmployerRef = source.EmployerRef,
                ProviderRef = source.ProviderRef,
                CanBeApproved = callerType == CallerType.Employer
                        ? source.EmployerCanApproveApprenticeship
                        : source.ProviderCanApproveApprenticeship,
                PendingUpdateOriginator = (Api.Types.Apprenticeship.Types.Originator?)source.UpdateOriginator,
                ProviderName = source.ProviderName,
                LegalEntityId = source.LegalEntityId,
                LegalEntityName = source.LegalEntityName,
                AccountLegalEntityPublicHashedId = source.AccountLegalEntityPublicHashedId,
                DataLockCourse = source.DataLocks.Any(x => x.WithCourseError() && x.TriageStatus == TriageStatus.Unknown),
                DataLockPrice = source.DataLocks.Any(x => x.IsPriceOnly() && x.TriageStatus == TriageStatus.Unknown),
                DataLockCourseTriaged = source.DataLocks.Any(x => x.WithCourseError() && x.TriageStatus == TriageStatus.Restart),
                DataLockCourseChangeTriaged = source.DataLocks.Any(x => x.WithCourseError() && x.TriageStatus == TriageStatus.Change),
                DataLockPriceTriaged = source.DataLocks.Any(x => x.IsPriceOnly() && x.TriageStatus == TriageStatus.Change),
                HasHadDataLockSuccess = source.HasHadDataLockSuccess,
                EndpointAssessorName = source.EndpointAssessorName,
                ReservationId = source.ReservationId
            };
        }

        public Domain.Entities.Apprenticeship Map(Apprenticeship apprenticeship, CallerType callerType)
        {
            var domainApprenticeship = new Domain.Entities.Apprenticeship
            {
                Id = apprenticeship.Id,
                FirstName = apprenticeship.FirstName,
                LastName = apprenticeship.LastName,
                DateOfBirth = apprenticeship.DateOfBirth,
                NINumber = apprenticeship.NINumber,
                ULN = apprenticeship.ULN,
                CommitmentId = apprenticeship.CommitmentId,
                PaymentStatus = (PaymentStatus)apprenticeship.PaymentStatus,
                AgreementStatus = (AgreementStatus)apprenticeship.AgreementStatus,
                TrainingType = (TrainingType)apprenticeship.TrainingType,
                TrainingCode = apprenticeship.TrainingCode,
                TrainingName = apprenticeship.TrainingName,
                Cost = apprenticeship.Cost,
                StartDate = apprenticeship.StartDate,
                EndDate = apprenticeship.EndDate,
                PauseDate = apprenticeship.PauseDate
                // We do not want to map reservation id as the reservation is not set in V1
            };

            if (callerType.IsEmployer())
                domainApprenticeship.EmployerRef = apprenticeship.EmployerRef;
            else
                domainApprenticeship.ProviderRef = apprenticeship.ProviderRef;

            return domainApprenticeship;
        }

        public IEnumerable<Apprenticeship> MapFrom(IEnumerable<ApprenticeshipResult> source)
        {
            return source.Select(sourceItem => new Apprenticeship
            {
                Id = sourceItem.Id,
                ULN = sourceItem.Uln,
                FirstName = sourceItem.FirstName,
                LastName = sourceItem.LastName,
                DateOfBirth = sourceItem.DateOfBirth,
                TrainingCode = sourceItem.TrainingCode,
                TrainingName = sourceItem.TrainingName,
                Cost = sourceItem.Cost,
                StartDate = sourceItem.StartDate,
                EndDate = sourceItem.EndDate,
                StopDate = sourceItem.StopDate,
                ProviderRef = sourceItem.ProviderRef,
                EmployerRef = sourceItem.EmployerRef,
                EmployerAccountId = sourceItem.EmployerAccountId,
                LegalEntityName = sourceItem.LegalEntityName,
                ProviderId = sourceItem.ProviderId,
                ProviderName = sourceItem.ProviderName,
                TransferSenderId = sourceItem.TransferSenderId,
                CommitmentId = sourceItem.CommitmentId
            });
        }

        public IEnumerable<Apprenticeship> MapFrom(IEnumerable<Domain.Entities.Apprenticeship> source, CallerType callerType)
        {
            return source.Select(sourceItem => MapFrom(sourceItem, callerType));
        }

        public IEnumerable<Apprenticeship> MapFromV2(IEnumerable<Domain.Entities.Apprenticeship> source, CallerType callerType)
        {
            return source.Select(sourceItem => MapFromV2(sourceItem, callerType));
        }

        public PriceHistory MapPriceHistory(Domain.Entities.PriceHistory domainPrice)
        {
            return new PriceHistory
            {
                ApprenticeshipId = domainPrice.ApprenticeshipId,
                Cost = domainPrice.Cost,
                FromDate = domainPrice.FromDate,
                ToDate = domainPrice.ToDate
            };
        }

        public ApprenticeshipUpdate MapApprenticeshipUpdate(Types.Apprenticeship.ApprenticeshipUpdate update)
        {
            var result = new ApprenticeshipUpdate
            {
                Id = update.Id,
                ApprenticeshipId = update.ApprenticeshipId,
                Originator = (Originator)update.Originator,
                FirstName = update.FirstName,
                LastName = update.LastName,
                DateOfBirth = update.DateOfBirth,
                TrainingCode = update.TrainingCode,
                TrainingType = update.TrainingType.HasValue ? (TrainingType)update.TrainingType : default(TrainingType?),
                TrainingName = update.TrainingName,
                Cost = update.Cost,
                StartDate = update.StartDate,
                EndDate = update.EndDate,
                UpdateOrigin = (UpdateOrigin)update.UpdateOrigin,
                EffectiveFromDate = update.EffectiveFromDate,
                EffectiveToDate = null,
                ULN = update.ULN,
                ProviderRef = update.ProviderRef,
                EmployerRef = update.EmployerRef
            };

            // Update the effective from date if they've made a change to the Start Date value - can only be done when waiting to start.
            if (update.StartDate.HasValue)
            {
                result.EffectiveFromDate = update.StartDate.Value;
            }

            return result;
        }

        public Types.Apprenticeship.ApprenticeshipUpdate MapApprenticeshipUpdate(ApprenticeshipUpdate data)
        {
            if (data == null)
            {
                return null;
            }

            return new Types.Apprenticeship.ApprenticeshipUpdate
            {
                Id = data.Id,
                ApprenticeshipId = data.ApprenticeshipId,
                Originator = (Types.Apprenticeship.Types.Originator)data.Originator,
                FirstName = data.FirstName,
                LastName = data.LastName,
                DateOfBirth = data.DateOfBirth,
                TrainingCode = data.TrainingCode,
                TrainingType = data.TrainingType.HasValue ? (Types.Apprenticeship.Types.TrainingType)data.TrainingType
                                                            : default(Types.Apprenticeship.Types.TrainingType?),
                TrainingName = data.TrainingName,
                Cost = data.Cost,
                StartDate = data.StartDate,
                EndDate = data.EndDate
            };
        }
    }
}
