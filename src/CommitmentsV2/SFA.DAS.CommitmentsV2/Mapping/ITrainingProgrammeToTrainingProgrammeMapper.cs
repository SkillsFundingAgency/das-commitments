using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;

namespace SFA.DAS.CommitmentsV2.Mapping
{
    public class ITrainingProgrammeToTrainingProgrammeMapper : IMapper<ITrainingProgramme, TrainingProgramme>
    {
        public TrainingProgramme Map(ITrainingProgramme source)
        {
            return new TrainingProgramme(source.Id, source.ExtendedTitle, source.ProgrammeType, source.EffectiveFrom, source.EffectiveTo);
        }
    }
}
