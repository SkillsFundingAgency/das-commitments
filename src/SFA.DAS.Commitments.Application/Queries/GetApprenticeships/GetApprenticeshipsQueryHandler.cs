using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

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
                    x => new Api.Types.Apprenticeship
                    {
                        Id = x.Id,
                        CommitmentId = x.CommitmentId,
                        EmployerAccountId = x.EmployerAccountId,
                        ProviderId = x.ProviderId,
                        Reference = x.Reference,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        ULN = x.ULN,
                        TrainingType = (Api.Types.TrainingType) x.TrainingType,
                        TrainingCode = x.TrainingCode,
                        TrainingName = x.TrainingName,
                        Cost = x.Cost,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        PaymentStatus = (Api.Types.PaymentStatus) x.PaymentStatus,
                        AgreementStatus = (Api.Types.AgreementStatus) x.AgreementStatus,
                        DateOfBirth = x.DateOfBirth,
                        NINumber = x.NINumber,
                        EmployerRef = x.EmployerRef,
                        ProviderRef = x.ProviderRef,
                        CanBeApproved = message.Caller.CallerType == CallerType.Employer ? x.EmployerCanApproveApprenticeship : x.ProviderCanApproveApprenticeship
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
