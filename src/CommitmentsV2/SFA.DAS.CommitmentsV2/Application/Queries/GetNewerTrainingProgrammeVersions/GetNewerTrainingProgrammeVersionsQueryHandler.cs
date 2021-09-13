using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetNewerTrainingProgrammeVersions
{
    public class GetNewerTrainingProgrammeVersionsQueryHandler : IRequestHandler<GetNewerTrainingProgrammeVersionsQuery, GetNewerTrainingProgrammeVersionsQueryResult>
    {
        private readonly ITrainingProgrammeLookup _trainingProgrammeService;
        private readonly ILogger<GetNewerTrainingProgrammeVersionsQueryHandler> _logger;

        public GetNewerTrainingProgrammeVersionsQueryHandler(ITrainingProgrammeLookup trainingProgrammeService, ILogger<GetNewerTrainingProgrammeVersionsQueryHandler> logger)
        {
            _trainingProgrammeService = trainingProgrammeService;
            _logger = logger;
        }

        public async Task<GetNewerTrainingProgrammeVersionsQueryResult> Handle(GetNewerTrainingProgrammeVersionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var newerVersions = await _trainingProgrammeService.GetNewerTrainingProgrammeVersions(request.StandardUId);

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
                        }).ToList()
                    })
                };
            }
            catch (Exception e)
            {
                _logger.LogInformation(e, $"Standard not found: {request.StandardUId}");
            }

            return new GetNewerTrainingProgrammeVersionsQueryResult
            {
                NewerVersions = null
            };
        }
    }
}
