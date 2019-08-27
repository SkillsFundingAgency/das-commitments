using System;
using MediatR;

namespace SFA.DAS.Commitments.Application.Commands.ValidateReservation
{
    /// <summary>
    ///     Request to validate the supplied apprenticeship. If values for course code or start date are not supplied 
    ///     then the command will use the values from the database.
    /// </summary>
    public sealed class ValidateReservationRequest : IAsyncRequest<ValidateReservationResponse>
    {
        public long ApprenticeshipId { get; set; }
        public string CourseCode { get; set; }
        public DateTime? StartDate { get; set; }
    }
}