using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.QueryExtensions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship
{
    public class GetSupportApprenticeshipQueryHandler : IRequestHandler<GetSupportApprenticeshipQuery, GetSupportApprenticeshipQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly IMapper<Apprenticeship, SupportApprenticeshipDetails> _mapper;

        public GetSupportApprenticeshipQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext,
            ICurrentDateTime currentDateTime,
            IMapper<Apprenticeship, SupportApprenticeshipDetails> mapper)
        {
            _dbContext = dbContext;
            _currentDateTime = currentDateTime;
            _mapper = mapper;
        }

        public async Task<GetSupportApprenticeshipQueryResult> Handle(GetSupportApprenticeshipQuery request, CancellationToken cancellationToken)
        {
            var query = _dbContext.Value
                .Apprenticeships
                .Include(x => x.Cohort)
                .Include(x => x.Cohort.AccountLegalEntity)
                .Include(x => x.Cohort.Provider)
                .Include(x => x.PriceHistory)
                .AsQueryable();

            if (request.ApprenticeshipId.HasValue)
            {
                query = query.Where(x => x.Id == request.ApprenticeshipId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Uln))
            {
                query = query.Where(x => x.Uln == request.Uln);
            }

            if (request.CohortId.HasValue)
            {
                query = query.Where(x => x.CommitmentId == request.CohortId.Value);
            }

            var apprenticeships = await query.ToListAsync(CancellationToken.None);

            var response = new GetSupportApprenticeshipQueryResult();

            foreach (var apprenticeship in apprenticeships)
            {
                var model = await _mapper.Map(apprenticeship);
                response.Apprenticeships.Add(model);
            }

            return response;
        }
    }
}