using System.Collections.Generic;
using System.Linq;

using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

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
                Reference = source.Reference,
                FirstName = source.FirstName,
                LastName = source.LastName,
                ULN = source.ULN,
                TrainingType = (Api.Types.Apprenticeship.Types.TrainingType) source.TrainingType,
                TrainingCode = source.TrainingCode,
                TrainingName = source.TrainingName,
                Cost = source.Cost,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                PaymentStatus = (Api.Types.Apprenticeship.Types.PaymentStatus) source.PaymentStatus,
                AgreementStatus = (Api.Types.AgreementStatus) source.AgreementStatus,
                DateOfBirth = source.DateOfBirth,
                NINumber = source.NINumber,
                EmployerRef = source.EmployerRef,
                ProviderRef = source.ProviderRef,
                CanBeApproved = callerType == CallerType.Employer
                        ? source.EmployerCanApproveApprenticeship
                        : source.ProviderCanApproveApprenticeship,
                PendingUpdateOriginator = (Api.Types.Apprenticeship.Types.Originator?) source.UpdateOriginator,
                ProviderName = source.ProviderName,
                LegalEntityId = source.LegalEntityId,
                LegalEntityName = source.LegalEntityName,
                DataLockCourse = source.DataLockCourse,
                DataLockPrice = source.DataLockPrice,
                DataLockCourseTriaged = source.DataLockCourseTriaged,
                DataLockPriceTriaged = source.DataLockPriceTriaged,
            };
        }

        public Domain.Entities.Apprenticeship Map(Apprenticeship apprenticeship, CallerType callerType)
        {
            // ToDo: Test
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
                EndDate = apprenticeship.EndDate
            };

            if (callerType.IsEmployer())
                domainApprenticeship.EmployerRef = apprenticeship.EmployerRef;
            else
                domainApprenticeship.ProviderRef = apprenticeship.ProviderRef;

            return domainApprenticeship;
        }



        public IEnumerable<Apprenticeship> MapFrom(IEnumerable<Domain.Entities.Apprenticeship> source, CallerType callerType)
        {
            return source.Select(sourceItem => MapFrom(sourceItem, callerType));
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
    }
}