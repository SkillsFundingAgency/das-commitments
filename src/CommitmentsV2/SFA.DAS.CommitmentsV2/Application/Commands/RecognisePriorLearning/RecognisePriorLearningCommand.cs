using MediatR;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.RecognisePriorLearning
{
    public class RecognisePriorLearningCommand : IRequest
    {
        public long CohortId { get; set; }
        public long ApprenticeshipId { get; set; }
        public bool? RecognisePriorLearning { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}
