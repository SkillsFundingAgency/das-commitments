
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgramme;

public class GetTrainingProgrammeQueryHandler(ITrainingProgrammeLookup service, ILogger<GetTrainingProgrammeQueryHandler> logger)
    : IRequestHandler<GetTrainingProgrammeQuery, GetTrainingProgrammeQueryResult>
{
    public async Task<GetTrainingProgrammeQueryResult> Handle(GetTrainingProgrammeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetTrainingProgramme(request.Id);

            if (result == null)
            {
                return new GetTrainingProgrammeQueryResult
                {
                    TrainingProgramme = null
                };
            }
            
            return new GetTrainingProgrammeQueryResult
            {
                TrainingProgramme = new TrainingProgramme
                {
                    Name = result.Name,
                    CourseCode = result.CourseCode,
                    EffectiveFrom = result.EffectiveFrom,
                    EffectiveTo = result.EffectiveTo,
                    ProgrammeType = result.ProgrammeType,
                    FundingPeriods = result.FundingPeriods.Select(x => new TrainingProgrammeFundingPeriod
                    {
                        EffectiveFrom = x.EffectiveFrom,
                        EffectiveTo = x.EffectiveTo,
                        FundingCap = x.FundingCap
                    }).ToList(),
                    StandardUId = result.StandardUId,
                    Version = result.Version,
                    Options = result.Options,
                    VersionEarliestStartDate = result.VersionEarliestStartDate,
                    VersionLatestStartDate = result.VersionLatestStartDate
                }
            }; 
        }
        catch (Exception exception)
        {
            logger.LogInformation(exception, "Course not found : {Id}", request.Id);
        }
        
        return new GetTrainingProgrammeQueryResult { TrainingProgramme = null };
    }
}