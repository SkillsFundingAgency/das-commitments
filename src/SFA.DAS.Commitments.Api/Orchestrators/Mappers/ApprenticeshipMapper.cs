using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Domain;

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
                CanBeApproved =
                    callerType == CallerType.Employer
                        ? source.EmployerCanApproveApprenticeship
                        : source.ProviderCanApproveApprenticeship,
                PendingUpdateOriginator = (Api.Types.Apprenticeship.Types.Originator?) source.UpdateOriginator,
                ProviderName = source.ProviderName,
                LegalEntityName = source.LegalEntityName,
                DataLockCourse = source.DataLockCourse,
                DataLockPrice = source.DataLockPrice,
                DataLockCourseTriaged = source.DataLockCourseTriaged,
                DataLockPriceTriaged = source.DataLockPriceTriaged,
            };
        }

        public IEnumerable<Apprenticeship> MapFrom(IEnumerable<Domain.Entities.Apprenticeship> source, CallerType callerType)
        {
            return source.Select(sourceItem => MapFrom(sourceItem, callerType));
        }
    }
}