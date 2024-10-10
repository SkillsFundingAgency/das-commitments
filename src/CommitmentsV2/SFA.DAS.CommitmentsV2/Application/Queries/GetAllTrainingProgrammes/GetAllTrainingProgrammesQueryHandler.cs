using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllTrainingProgrammes;

public class GetAllTrainingProgrammesQueryHandler(ITrainingProgrammeLookup service) : IRequestHandler<GetAllTrainingProgrammesQuery, GetAllTrainingProgrammesQueryResult>
{
    public async Task<GetAllTrainingProgrammesQueryResult> Handle(GetAllTrainingProgrammesQuery request, CancellationToken cancellationToken)
    {
        var result = await service.GetAll();
            
        return new GetAllTrainingProgrammesQueryResult
        {
            TrainingProgrammes = result.Select(c=> new TrainingProgramme
            {
                Name = c.Name,
                CourseCode = c.CourseCode,
                EffectiveFrom = c.EffectiveFrom,
                EffectiveTo = c.EffectiveTo,
                ProgrammeType = c.ProgrammeType,
                FundingPeriods = c.FundingPeriods.Select(x=>new TrainingProgrammeFundingPeriod
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