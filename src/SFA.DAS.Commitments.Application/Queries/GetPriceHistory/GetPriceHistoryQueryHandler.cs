using System;
using System.Linq;
using System.Threading.Tasks;

using MediatR;

using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Queries.GetPriceHistory
{
    public class GetPriceHistoryQueryHandler : IAsyncRequestHandler<GetPriceHistoryRequest, GetPriceHistoryResponse>
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;

        public GetPriceHistoryQueryHandler(IApprenticeshipRepository apprenticeshipRepository)
        {
            if(apprenticeshipRepository == null)
                throw new ArgumentNullException($"{nameof(IApprenticeshipUpdateRepository)} cannot be null");

            _apprenticeshipRepository = apprenticeshipRepository;
        }

        public async Task<GetPriceHistoryResponse> Handle(GetPriceHistoryRequest message)
        {
            var priceHistoryItems = 
                (await _apprenticeshipRepository.GetPriceHistory(message.ApprenticeshipId))
                .Select(m => 
                    new PriceHistory
                    {
                        ApprenticeshipId = m.ApprenticeshipId,
                        Cost = m.Cost, 
                        FromDate = m.FromDate,
                        ToDate = m.ToDate
                    });

            return new 
                GetPriceHistoryResponse
                {
                    Data = priceHistoryItems
                };
        }
    }
}