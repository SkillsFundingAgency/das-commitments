using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using TrainingProgrammeEntity = SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersion;

public class GetTrainingProgrammeVersionQueryHandler(ITrainingProgrammeLookup service, ILogger<GetTrainingProgrammeVersionQueryHandler> logger)
    : IRequestHandler<GetTrainingProgrammeVersionQuery, GetTrainingProgrammeVersionQueryResult>
{
    public async Task<GetTrainingProgrammeVersionQueryResult> Handle(GetTrainingProgrammeVersionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            TrainingProgrammeEntity result;

            if (request.StandardUId != null)
            {
                result = await service.GetTrainingProgrammeVersionByStandardUId(request.StandardUId);
            }
            else
            {
                result = await service.GetTrainingProgrammeVersionByCourseCodeAndVersion(request.CourseCode, request.Version);
            }

            if (result == null)
            {
                return new GetTrainingProgrammeVersionQueryResult
                {
                    TrainingProgramme = null
                };
            }

            return new GetTrainingProgrammeVersionQueryResult
            {
                TrainingProgramme = new TrainingProgramme
                {
                    Name = result.Name,
                    CourseCode = result.CourseCode,
                    StandardUId = result.StandardUId,
                    Version = result.Version,
                    Level = result.Level,
                    StandardPageUrl = result.StandardPageUrl,
                    EffectiveFrom = result.EffectiveFrom,
                    EffectiveTo = result.EffectiveTo,
                    ProgrammeType = result.ProgrammeType,
                    Options = result.Options,
                    FundingPeriods = result.FundingPeriods.Select(x => new TrainingProgrammeFundingPeriod
                    {
                        EffectiveFrom = x.EffectiveFrom,
                        EffectiveTo = x.EffectiveTo,
                        FundingCap = x.FundingCap
                    }).ToList(),
                    VersionEarliestStartDate = result.VersionEarliestStartDate,
                    VersionLatestStartDate = result.VersionLatestStartDate
                }
            };
        }
        catch (Exception e)
        {
            logger.LogInformation(e, "Standard not found: {StandardUId}", request.StandardUId);
        }
        return new GetTrainingProgrammeVersionQueryResult
        {
            TrainingProgramme = null
        };
    }
}