using MediatR;
using System;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersion
{
    public class GetTrainingProgrammeVersionQuery : IRequest<GetTrainingProgrammeVersionQueryResult>
    {
        public int CourseCode { get; set; }
        public DateTime StartDate { get; set; }
    }
}
