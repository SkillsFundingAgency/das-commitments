using System;
using System.Reflection;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Mapping
{
    public class ITrainingProgrammeToTrainingProgrammeMapper : IMapper<ITrainingProgramme, TrainingProgramme>
    {
        public TrainingProgramme Map(ITrainingProgramme source)
        {
            return new TrainingProgramme(source.Id, source.ExtendedTitle, MapProgrameTypesToV2ProgrammeTypes(source.ProgrammeType), source.EffectiveFrom, source.EffectiveTo);
        }

        private static TrainingType MapProgrameTypesToV2ProgrammeTypes(ProgrammeType programmeType)
        {
            switch (programmeType)
            {
                case ProgrammeType.Framework:
                    return TrainingType.Framework;
                case ProgrammeType.Standard:
                    return TrainingType.Standard;
                default:
                    throw new AmbiguousMatchException("Could not map ProgrammeType");
            }
        }

    }
}
