using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersion
{
    public class GetTrainingProgrammeVersionQueryHandler : IRequestHandler<GetTrainingProgrammeVersionQuery, GetTrainingProgrammeVersionQueryResult>
    {
        private readonly ITrainingProgrammeLookup _service;
        private readonly ILogger<GetTrainingProgrammeVersionQueryHandler> _logger;

        public GetTrainingProgrammeVersionQueryHandler(ITrainingProgrammeLookup service, ILogger<GetTrainingProgrammeVersionQueryHandler> logger)
        {
            _service = service;
            _logger = logger;
        }
        public async Task<GetTrainingProgrammeVersionQueryResult> Handle(GetTrainingProgrammeVersionQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _service.GetTrainingProgrammeVersionByStandardUId(request.StandardUId);

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
                        EffectiveFrom = result.EffectiveFrom,
                        EffectiveTo = result.EffectiveTo,
                        ProgrammeType = result.ProgrammeType,
                        Options = result.Options,
                        FundingPeriods = result.FundingPeriods.Select(x => new TrainingProgrammeFundingPeriod
                        {
                            EffectiveFrom = x.EffectiveFrom,
                            EffectiveTo = x.EffectiveTo,
                            FundingCap = x.FundingCap
                        }).ToList()
                    }
                };
            }
            catch (Exception e)
            {
                _logger.LogInformation(e, $"Standard not found: {request.StandardUId}");
            }
            return new GetTrainingProgrammeVersionQueryResult
            {
                TrainingProgramme = null
            };
        }
    }
}
