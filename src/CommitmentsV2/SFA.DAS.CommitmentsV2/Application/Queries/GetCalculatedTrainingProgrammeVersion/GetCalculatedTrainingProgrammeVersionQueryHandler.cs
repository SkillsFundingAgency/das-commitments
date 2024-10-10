using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCalculatedTrainingProgrammeVersion;

public class GetCalculatedTrainingProgrammeVersionQueryHandler(ITrainingProgrammeLookup service) : IRequestHandler<GetCalculatedTrainingProgrammeVersionQuery, GetCalculatedTrainingProgrammeVersionQueryResult>
{
    public async Task<GetCalculatedTrainingProgrammeVersionQueryResult> Handle(GetCalculatedTrainingProgrammeVersionQuery request, CancellationToken cancellationToken)
    {
        var result = await service.GetCalculatedTrainingProgrammeVersion(request.CourseCode.ToString(), request.StartDate);

        if (result == null)
        {
            return new GetCalculatedTrainingProgrammeVersionQueryResult
            {
                TrainingProgramme = null
            };
        }

        return new GetCalculatedTrainingProgrammeVersionQueryResult
        {
            TrainingProgramme = new TrainingProgramme
            {
                Name = result.Name,
                CourseCode = result.CourseCode,
                StandardUId = result.StandardUId,
                Version = result.Version,
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
}