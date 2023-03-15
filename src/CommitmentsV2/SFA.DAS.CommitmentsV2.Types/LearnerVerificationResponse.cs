using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Types
{
    public enum LearnerVerificationResponseType
    {
        SuccessfulMatch,

        SuccessfulLinkedMatch,

        SimilarMatch,

        SimilarLinkedMatch,

        LearnerDoesNotMatch,

        UlnNotFound
    }

    public enum LearnerDetailMatchingError
    {
        GivenDoesntMatchGiven,

        GivenDoesntMatchFamily,

        GivenDoesntMatchPreviousFamily,

        FamilyDoesntMatchGiven,

        FamilyDoesntMatchFamily,

        FamilyDoesntMatchPreviousFamily,

        DateOfBirthDoesntMatchDateOfBirth,

        GenderDoesntMatchGender
    }

    public class LearnerVerificationResponse
    {
        public LearnerVerificationResponseType ResponseType { get; set; }

        public IEnumerable<LearnerDetailMatchingError> MatchingErrors { get; set; }
    }
}