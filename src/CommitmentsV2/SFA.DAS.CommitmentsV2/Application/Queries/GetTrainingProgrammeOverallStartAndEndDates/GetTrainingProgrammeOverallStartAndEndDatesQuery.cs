using MediatR;
using System;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCalculatedTrainingProgrammeVersion
{
    public class GetTrainingProgrammeOverallStartAndEndDatesQuery : IRequest<GetTrainingProgrammeOverallStartAndEndDatesQueryResult>
    {
        public string CourseCode { get; set; }
    }
}
