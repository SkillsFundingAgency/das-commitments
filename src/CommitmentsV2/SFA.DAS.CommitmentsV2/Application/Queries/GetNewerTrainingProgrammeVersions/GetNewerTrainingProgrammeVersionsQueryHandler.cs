﻿
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetNewerTrainingProgrammeVersions;

public class GetNewerTrainingProgrammeVersionsQueryHandler(ITrainingProgrammeLookup trainingProgrammeService, ILogger<GetNewerTrainingProgrammeVersionsQueryHandler> logger)
    : IRequestHandler<GetNewerTrainingProgrammeVersionsQuery, GetNewerTrainingProgrammeVersionsQueryResult>
{
    public async Task<GetNewerTrainingProgrammeVersionsQueryResult> Handle(GetNewerTrainingProgrammeVersionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var newerVersions = await trainingProgrammeService.GetNewerTrainingProgrammeVersions(request.StandardUId);

            if (newerVersions == null)
            {
                return new GetNewerTrainingProgrammeVersionsQueryResult
                {
                    NewerVersions = null
                };
            }

            return new GetNewerTrainingProgrammeVersionsQueryResult
            {
                NewerVersions = newerVersions.Select(version => new TrainingProgramme
                {
                    Name = version.Name,
                    CourseCode = version.CourseCode,
                    StandardUId = version.StandardUId,
                    Version = version.Version,
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
            logger.LogInformation(exception, "Standard not found: {StandardUId}", request.StandardUId);
        }

        return new GetNewerTrainingProgrammeVersionsQueryResult
        {
            NewerVersions = null
        };
    }
}