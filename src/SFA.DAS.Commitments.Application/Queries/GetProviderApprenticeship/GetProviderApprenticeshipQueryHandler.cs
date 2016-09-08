using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Queries.GetProviderApprenticeship
{
    public class GetProviderApprenticeshipQueryHandler : IAsyncRequestHandler<GetProviderApprenticeshipQueryRequest, GetProviderApprenticeshipQueryResponse>
    {
        private readonly ICommitmentRepository _commitmentRepository;

        public GetProviderApprenticeshipQueryHandler(ICommitmentRepository commitmentRepository)
        {
            if (commitmentRepository == null)
                throw new ArgumentNullException(nameof(commitmentRepository));
            _commitmentRepository = commitmentRepository;
        }

        public async Task<GetProviderApprenticeshipQueryResponse> Handle(GetProviderApprenticeshipQueryRequest message)
        {
            var apprenticeship = await _commitmentRepository.GetApprenticeship(message.ApprenticeshipId);

            return new GetProviderApprenticeshipQueryResponse
            {
                Data = new Apprenticeship
                {
                    Id = apprenticeship.Id,
                    CommitmentId = apprenticeship.CommitmentId,
                    ULN = apprenticeship.ULN,
                    FirstName = apprenticeship.FirstName,
                    LastName = apprenticeship.LastName,
                    TrainingId = apprenticeship.TrainingId,
                    StartDate = apprenticeship.StartDate,
                    EndDate = apprenticeship.EndDate,
                    Cost = apprenticeship.Cost,
                    Status = (ApprenticeshipStatus)apprenticeship.Status,
                    AgreementStatus = (AgreementStatus)apprenticeship.AgreementStatus
                }
            };
        }
    }
}