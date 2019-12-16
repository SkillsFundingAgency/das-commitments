using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprentices
{
    public class GetApprovedApprenticesHandler : IRequestHandler<GetApprovedApprenticesRequest, GetApprovedApprenticesResponse>
    {
        private readonly IProviderCommitmentsDbContext _dbContext;

        public GetApprovedApprenticesHandler(IProviderCommitmentsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetApprovedApprenticesResponse> Handle(GetApprovedApprenticesRequest request, CancellationToken cancellationToken)
        {
            var matched = await _dbContext.ApprovedApprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Include(apprenticeship => apprenticeship.DataLockStatus)
                .Where(apprenticeship => apprenticeship.ProviderRef == request.ProviderId.ToString())
                .Select(apprenticeship => (ApprenticeshipDetails)apprenticeship)
                .ToListAsync(cancellationToken);

            return new GetApprovedApprenticesResponse
            {
                Apprenticeships = matched
            };
        }
    }
}
