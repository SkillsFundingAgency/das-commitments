using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

using SFA.DAS.Commitments.Api.Types.DataLock.Types;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using AgreementStatus = SFA.DAS.Commitments.Api.Types.AgreementStatus;
using Originator = SFA.DAS.Commitments.Api.Types.Apprenticeship.Types.Originator;
using PaymentStatus = SFA.DAS.Commitments.Api.Types.Apprenticeship.Types.PaymentStatus;
using TrainingType = SFA.DAS.Commitments.Api.Types.Apprenticeship.Types.TrainingType;

namespace SFA.DAS.Commitments.Application.Queries.GetApprenticeships
{
    public sealed class GetApprenticeshipsQueryHandler : IAsyncRequestHandler<GetApprenticeshipsRequest, GetApprenticeshipsResponse>
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;

        public GetApprenticeshipsQueryHandler(IApprenticeshipRepository apprenticeshipRepository)
        {
            _apprenticeshipRepository = apprenticeshipRepository;
        }

        public async Task<GetApprenticeshipsResponse> Handle(GetApprenticeshipsRequest message)
        {
            var apprenticeships = await GetApprenticeships(message.Caller);

            if (apprenticeships == null)
            {
                return new GetApprenticeshipsResponse();
            }

            return new GetApprenticeshipsResponse
            {
                Data = apprenticeships.Select(
                    x => new Api.Types.Apprenticeship.Apprenticeship
                    {
                        Id = x.Id,
                        CommitmentId = x.CommitmentId,
                        EmployerAccountId = x.EmployerAccountId,
                        ProviderId = x.ProviderId,
                        Reference = x.Reference,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        ULN = x.ULN,
                        TrainingType = (TrainingType) x.TrainingType,
                        TrainingCode = x.TrainingCode,
                        TrainingName = x.TrainingName,
                        Cost = x.Cost,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        PaymentStatus = (PaymentStatus) x.PaymentStatus,
                        AgreementStatus = (AgreementStatus) x.AgreementStatus,
                        DateOfBirth = x.DateOfBirth,
                        NINumber = x.NINumber,
                        EmployerRef = x.EmployerRef,
                        ProviderRef = x.ProviderRef,
                        CanBeApproved = message.Caller.CallerType == CallerType.Employer ? x.EmployerCanApproveApprenticeship : x.ProviderCanApproveApprenticeship,
                        PendingUpdateOriginator = (Originator?)x.UpdateOriginator,
                        ProviderName = x.ProviderName,
                        LegalEntityName = x.LegalEntityName,
                        DataLockTriageStatus = (TriageStatus?)x.DataLockTriage,
                        DataLockErrorCode = (DataLockErrorCode)x.DataLockErrorCode,
                        LegalEntityId = x.LegalEntityId
                    }
                    ).ToList()
            };
        }

        private async Task<IList<Apprenticeship>> GetApprenticeships(Caller caller)
        {
            switch (caller.CallerType)
            {
                case CallerType.Employer:
                    return await _apprenticeshipRepository.GetApprenticeshipsByEmployer(caller.Id);
                case CallerType.Provider:
                    return await _apprenticeshipRepository.GetApprenticeshipsByProvider(caller.Id);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
