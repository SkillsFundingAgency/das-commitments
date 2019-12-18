using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprentices
{
    public class GetApprovedApprenticesHandler : IRequestHandler<GetApprovedApprenticesRequest, GetApprovedApprenticesResponse>
    {
        private readonly IProviderCommitmentsDbContext _dbContext;
        private readonly IMapper<Apprenticeship, ApprenticeshipDetails> _mapper;

        public GetApprovedApprenticesHandler(
            IProviderCommitmentsDbContext dbContext,
            IMapper<Apprenticeship, ApprenticeshipDetails> mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<GetApprovedApprenticesResponse> Handle(GetApprovedApprenticesRequest request, CancellationToken cancellationToken)
        {
            var mapped = new List<ApprenticeshipDetails>();

            var matched = await _dbContext.Apprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Include(apprenticeship => apprenticeship.DataLockStatus)
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == request.ProviderId)
                .ToListAsync(cancellationToken);

            foreach (var apprenticeship in matched)
            {
                var details = await _mapper.Map(apprenticeship);
                mapped.Add(details);
            }

            return new GetApprovedApprenticesResponse
            {
                Apprenticeships = mapped
            };
        }
    }
}
