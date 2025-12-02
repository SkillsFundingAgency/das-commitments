
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersions;

public class GetTrainingProgrammeVersionsQueryHandler(ITrainingProgrammeLookup trainingProgrammeService, ILogger<GetTrainingProgrammeVersionsQueryHandler> logger)
    : IRequestHandler<GetTrainingProgrammeVersionsQuery, GetTrainingProgrammeVersionsQueryResult>
{
    public async Task<GetTrainingProgrammeVersionsQueryResult> Handle(GetTrainingProgrammeVersionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await trainingProgrammeService.GetTrainingProgrammeVersions(request.Id);

            if (result == null)
            {
                return new GetTrainingProgrammeVersionsQueryResult
                {
                    TrainingProgrammes = null
                };
            }

            return new GetTrainingProgrammeVersionsQueryResult
            {
                TrainingProgrammes = result.Select(version => new TrainingProgramme
                {
                    Name = version.Name,
                    CourseCode = version.CourseCode,
                    StandardUId = version.StandardUId,
                    Version = version.Version,
                    Level = version.Level,
                    StandardPageUrl = version.StandardPageUrl,
                    EffectiveFrom = version.EffectiveFrom,
                    EffectiveTo = version.EffectiveTo,
                    ProgrammeType = version.ProgrammeType,
                    Options = version.Options,
                    FundingPeriods = version.FundingPeriods.Select(x => new TrainingProgrammeFundingPeriod
                    {
                        EffectiveFrom = x.EffectiveFrom,
                        EffectiveTo = x.EffectiveTo,
                        FundingCap = x.FundingCap
                    }).ToList(),
                        VersionEarliestStartDate = version.VersionEarliestStartDate,
                        VersionLatestStartDate = version.VersionLatestStartDate
                })
            };
        }
        catch (Exception exception)
        {
            logger.LogInformation(exception, "Standard not found: {request.Id}", request.Id);
        }

        return new GetTrainingProgrammeVersionsQueryResult
        {
            TrainingProgrammes = null
        };
    }
}