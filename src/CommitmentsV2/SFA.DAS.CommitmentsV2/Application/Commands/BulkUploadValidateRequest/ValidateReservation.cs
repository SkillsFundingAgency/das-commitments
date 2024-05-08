using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;

public partial class BulkUploadValidateCommandHandler
{
    private static IEnumerable<Error> ValidateReservation(BulkUploadAddDraftApprenticeshipRequest csvRecord, BulkReservationValidationResults reservationValidationResults)
    {
        var domainErrors = new List<Error>();
        var reservationValidationError = reservationValidationResults?.ValidationErrors?.Where(x => x.RowNumber == csvRecord.RowNumber);
        if (reservationValidationError != null && reservationValidationError.Any())
        {
            domainErrors.AddRange(reservationValidationError.Select(y => new Error("ReservationId", y.Reason)));
        }

        return domainErrors;
    }
}