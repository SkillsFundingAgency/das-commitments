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
            var apprenticeshipsResult = await GetApprenticeships(message.Caller, message.SearchKeyword);

            return new GetApprenticeshipsResponse
            {
                Apprenticeships = apprenticeshipsResult.Apprenticeships,
                TotalCount = apprenticeshipsResult.TotalCount
            };
        }

        private async Task<ApprenticeshipsResult> GetApprenticeships(Caller caller, string searchKeyword)
        {
            switch (caller.CallerType)
            {
                case CallerType.Employer:
                    return await _apprenticeshipRepository.GetApprenticeshipsByEmployer(caller.Id, searchKeyword);
                case CallerType.Provider:
                    return await _apprenticeshipRepository.GetApprenticeshipsByProvider(caller.Id, searchKeyword);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
