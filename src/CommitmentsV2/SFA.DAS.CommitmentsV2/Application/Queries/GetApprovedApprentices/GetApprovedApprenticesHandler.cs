using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprentices
{
    public class GetApprovedApprenticesHandler : IRequestHandler<GetApprovedApprenticesRequest, GetApprovedApprenticesResponse>
    {
        public Task<GetApprovedApprenticesResponse> Handle(GetApprovedApprenticesRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new GetApprovedApprenticesResponse
            {
                Apprenticeships = new[]
                {
                    new ApprenticeshipDetails
                    {
                        ApprenticeName = "Mr Test",
                        Uln = "12345",
                        EmployerName = "Test Corp",
                        CourseName = "Testing Level 1",
                        PlannedStartDate = DateTime.Now.AddDays(2),
                        PlannedEndDateTime = DateTime.Now.AddMonths(2),
                        Status = "Planned",
                        Alerts = "Test Alert"
                    }
                }
            });
        }
    }
}
