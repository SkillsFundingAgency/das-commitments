using MediatR;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.PriorLearningData
{
    public class PriorLearningDataCommand : IRequest
    {
        public long CohortId { get; set; }
        public long ApprenticeshipId { get; set; }
        public int? TrainingTotalHours { get; set; }
        public int? DurationReducedByHours { get; set; }
        public bool? IsDurationReducedByRpl { get; set; }
        public int? DurationReducedBy { get; set; }
        public int? CostBeforeRpl { get; set; }
        public int? PriceReducedBy { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}
