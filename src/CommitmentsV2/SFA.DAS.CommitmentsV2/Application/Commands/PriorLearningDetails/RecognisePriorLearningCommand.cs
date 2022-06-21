using MediatR;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.PriorLearningDetails
{
    public class PriorLearningDetailsCommand : IRequest
    {
        public long CohortId { get; set; }
        public long ApprenticeshipId { get; set; }
        public int? DurationReducedBy { get; set; }
        public int? PriceReducedBy { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}
