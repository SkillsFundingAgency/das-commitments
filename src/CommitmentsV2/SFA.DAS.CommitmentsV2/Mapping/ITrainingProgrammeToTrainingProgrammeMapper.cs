using System;
using System.Reflection;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using ProgrammeType = SFA.DAS.CommitmentsV2.Types.ProgrammeType;

namespace SFA.DAS.CommitmentsV2.Mapping
{
    public class ITrainingProgrammeToTrainingProgrammeMapper : IMapper<ITrainingProgramme, TrainingProgramme>
    {
        public TrainingProgramme Map(ITrainingProgramme source)
        {
            return new TrainingProgramme(source.Id, source.ExtendedTitle, MapProgrameTypesToV2ProgrammeTypes(source.ProgrammeType), source.EffectiveFrom, source.EffectiveTo);
        }

        private static ProgrammeType MapProgrameTypesToV2ProgrammeTypes(Apprenticeships.Api.Types.ProgrammeType programmeType)
        {
            switch (programmeType)
            {
                case Apprenticeships.Api.Types.ProgrammeType.Framework:
                    return ProgrammeType.Framework;
                case Apprenticeships.Api.Types.ProgrammeType.Standard:
                    return ProgrammeType.Standard;
                default:
                    throw new AmbiguousMatchException("Could not map ProgrammeType");
            }
        }

    }
}

