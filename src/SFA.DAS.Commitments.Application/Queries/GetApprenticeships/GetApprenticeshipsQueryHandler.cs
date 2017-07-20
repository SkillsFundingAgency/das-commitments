using System;
using System.Collections.Generic;
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

            return new GetApprenticeshipsResponse
            {
                Data = apprenticeships
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
