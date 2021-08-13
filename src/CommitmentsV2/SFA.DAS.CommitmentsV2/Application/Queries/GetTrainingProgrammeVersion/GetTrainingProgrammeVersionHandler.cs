using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersion
{
    public class GetTrainingProgrammeVersionHandler : IRequestHandler<GetTrainingProgrammeVersionQuery, GetTrainingProgrammeVersionResult>
    {
        private readonly ITrainingProgrammeLookup _service;
        private readonly ILogger<GetTrainingProgrammeVersionHandler> _logger;

        public GetTrainingProgrammeVersionHandler(ITrainingProgrammeLookup service, ILogger<GetTrainingProgrammeVersionHandler> logger)
        {
            _service = service;
            _logger = logger;
        }

        public async Task<GetTrainingProgrammeVersionResult> Handle(GetTrainingProgrammeVersionQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _service.GetTrainingProgrammeVersionByStandardUId(request.StandardUId);

                return new GetTrainingProgrammeVersionResult
                {
                    TrainingProgramme = new TrainingProgramme
                    {
                        Name = result.Name,
                        CourseCode = result.CourseCode,
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
                _logger.LogInformation(e, $"Standard version not found: {request.StandardUId}");

                return new GetTrainingProgrammeVersionResult
                {
                    TrainingProgramme = null
                };
            }
        }
    }
}
