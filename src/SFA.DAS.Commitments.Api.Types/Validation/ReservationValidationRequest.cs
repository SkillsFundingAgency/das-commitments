using System;

namespace SFA.DAS.Commitments.Api.Types.Validation
{
    /// <summary>
    ///     The request re-validates the existing reservation associated with the specified apprenticeship. Updated
    ///     course code and start date may be supplied on this request. If not supplied the existing details for the
    ///     apprenticeship will be used.
    /// </summary>
    public class ReservationValidationRequest
    {
        public long ApprenticeshipId { get; set; }
        public string ProposedCourseCode { get; set; }
        public DateTime? ProposedStartDate { get; set; }
    }
}