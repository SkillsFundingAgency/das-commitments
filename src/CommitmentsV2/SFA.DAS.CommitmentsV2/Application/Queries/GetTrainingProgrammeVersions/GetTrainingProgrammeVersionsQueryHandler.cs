﻿using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersions
{
    public class GetTrainingProgrammeVersionsQueryHandler : IRequestHandler<GetTrainingProgrammeVersionsQuery, GetTrainingProgrammeVersionsQueryResult>
    {
        private readonly ITrainingProgrammeLookup _trainingProgrammeService;
        private readonly ILogger<GetTrainingProgrammeVersionsQueryHandler> _logger;

        public GetTrainingProgrammeVersionsQueryHandler(ITrainingProgrammeLookup trainingProgrammeService, ILogger<GetTrainingProgrammeVersionsQueryHandler> logger)
        {
            _trainingProgrammeService = trainingProgrammeService;
            _logger = logger;
        }

        public async Task<GetTrainingProgrammeVersionsQueryResult> Handle(GetTrainingProgrammeVersionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _trainingProgrammeService.GetTrainingProgrammeVersions(request.Id);

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
                _logger.LogInformation(e, $"Standard not found: {request.Id}");
            }

            return new GetTrainingProgrammeVersionsQueryResult
            {
                TrainingProgrammes = null
            };
        }
    }
}
