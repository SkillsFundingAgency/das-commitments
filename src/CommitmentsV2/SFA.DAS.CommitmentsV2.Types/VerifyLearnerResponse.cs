using System.Collections.Generic;
using System.ComponentModel;

namespace SFA.DAS.CommitmentsV2.Types
{
    public enum LearnerValidationServiceResponseCode
    {
        [Description("WSVRC001")]
        SuccessfulMatch,

        [Description("WSVRC002")]
        SuccessfulLinkedMatch,

        [Description("WSVRC003")]
        SimilarMatch,

        [Description("WSVRC004")]
        SimilarLinkedMatch,

        [Description("WSVRC005")]
        LearnerDoesNotMatch,

        [Description("WSVRC006")]
        UlnNotFound
    }

    public enum FailureFlag
    {
        [Description("VRF1")]
        GivenDoesntMatchGiven,

        [Description("VRF2")]
        GivenDoesntMatchFamily,

        [Description("VRF3")]
        GivenDoesntMatchPreviousFamily,

        [Description("VRF4")]
        FamilyDoesntMatchGiven,

        [Description("VRF5")]
        FamilyDoesntMatchFamily,

        [Description("VRF6")]
        FamilyDoesntMatchPreviousFamily,

        [Description("VRF7")]
        DateOfBirthDoesntMatchDateOfBirth,

        [Description("VRF8")]
        GenderDoesntMatchGender
    }

    public class VerifyLearnerResponse
    {
        public LearnerValidationServiceResponseCode ResponseCode { get; set; }

        public IEnumerable<FailureFlag> FailureFlags { get; set; }
    }
}