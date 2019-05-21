using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.QueryExtensions;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprentice
{
    public class GetDraftApprenticeHandler : IRequestHandler<GetDraftApprenticeRequest, GetDraftApprenticeResponse>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetDraftApprenticeHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetDraftApprenticeResponse> Handle(GetDraftApprenticeRequest request, CancellationToken cancellationToken)
        {

            var result = await _dbContext.Value
                .DraftApprenticeships.GetById(
                    request.CohortId,
                    request.DraftApprenticeshipId,
                    draft => new {
                        CourseDetails = new {
                            draft.CourseCode,
                            draft.CourseName,
                            draft.ProgrammeType
                        },
                        DraftApprenticeshipDetails =  new DraftApprenticeshipDetails
                        {
                            StartDate = draft.StartDate,
                            Id = draft.Id,
                            Cost = (int)draft.Cost,
                            DateOfBirth = draft.DateOfBirth,
                            EndDate = draft.EndDate,
                            FirstName = draft.FirstName,
                            LastName = draft.LastName,
                            Reference = draft.ProviderRef,
                            ReservationId = draft.ReservationId,
                            Uln = draft.Uln
                        }
                    },
                    cancellationToken);

            if (result.CourseDetails.ProgrammeType != null)
            {
                result.DraftApprenticeshipDetails.TrainingProgramme = new TrainingProgramme(
                    result.CourseDetails.CourseCode,
                    result.CourseDetails.CourseName,
                    result.CourseDetails.ProgrammeType.Value,
                    null,
                    null);
            }

            return new GetDraftApprenticeResponse {DraftApprenticeshipDetails = result.DraftApprenticeshipDetails};
        }
    }
}
