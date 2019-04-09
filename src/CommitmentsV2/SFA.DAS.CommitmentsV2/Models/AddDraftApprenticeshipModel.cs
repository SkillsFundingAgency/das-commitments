using SFA.DAS.CommitmentsV2.Domain.ValueObjects;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class AddDraftApprenticeshipModel
    {
        public AddDraftApprenticeshipModel(Commitment commitment, DraftApprenticeshipDetails draftApporApprenticeshipDetails)
        {
            Commitment = commitment;
            DraftApprenticeshipDetails = draftApporApprenticeshipDetails;
        }

        public  Commitment Commitment { get; }
        public DraftApprenticeshipDetails DraftApprenticeshipDetails { get; }
    }
}