using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllTrainingProgrammeStandards;

public class GetAllTrainingProgrammeStandardsQueryHandler(ITrainingProgrammeLookup service) : IRequestHandler<GetAllTrainingProgrammeStandardsQuery,
    GetAllTrainingProgrammeStandardsQueryResult>
{
    public async Task<GetAllTrainingProgrammeStandardsQueryResult> Handle(
        GetAllTrainingProgrammeStandardsQuery request, CancellationToken cancellationToken)
    {
        var result = await service.GetAllStandards();
        
        return new GetAllTrainingProgrammeStandardsQueryResult
        {
            TrainingProgrammes = result.Select(c => new TrainingProgramme
            {
                Name = c.Name,
                CourseCode = c.CourseCode,
                EffectiveFrom = c.EffectiveFrom,
                EffectiveTo = c.EffectiveTo,
                ProgrammeType = c.ProgrammeType,
                StandardUId = c.StandardUId,
                StandardPageUrl = c.StandardPageUrl,
                FundingPeriods = c.FundingPeriods.Select(x => new TrainingProgrammeFundingPeriod
                {
                    EffectiveFrom = x.EffectiveFrom,
                    EffectiveTo = x.EffectiveTo,
                    FundingCap = x.FundingCap
                }).ToList(),
                VersionEarliestStartDate = c.VersionEarliestStartDate,
                VersionLatestStartDate = c.VersionLatestStartDate
            })
        };
    }
}