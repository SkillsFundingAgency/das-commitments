using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgramme
{
    public class GetTrainingProgrammeQueryHandler : IRequestHandler<GetTrainingProgrammeQuery, GetTrainingProgrammeQueryResult>
    {
        private readonly ITrainingProgrammeLookup _service;
        private readonly ILogger<GetTrainingProgrammeQueryHandler> _logger;

        public GetTrainingProgrammeQueryHandler (ITrainingProgrammeLookup service, ILogger<GetTrainingProgrammeQueryHandler> logger)
        {
            _service = service;
            _logger = logger;
        }
        
        public async Task<GetTrainingProgrammeQueryResult> Handle(GetTrainingProgrammeQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _service.GetTrainingProgramme(request.Id);

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
                        Options = result.Options
                    }
                }; 
            }
            catch (Exception e)
            {
                _logger.LogInformation(e, $"Course not found : {request.Id}");
            }
            return new GetTrainingProgrammeQueryResult
            {
                TrainingProgramme = null
            };
                
        }
    }
}