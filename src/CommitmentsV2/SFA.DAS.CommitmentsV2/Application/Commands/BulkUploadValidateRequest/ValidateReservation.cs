using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.Reservations.Api.Types;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
    {
        private async Task ValidateReservation(IEnumerable<BulkUploadAddDraftApprenticeshipRequest> csvRecords, 
            BulkReservationValidationResults reservationValidationResults, 
            long providerId, 
            List<BulkUploadValidationError> bulkUploadValidationErrors)
        {
            if (reservationValidationResults?.ValidationErrors?.Any() ?? false)
            {
                foreach (var validationError in reservationValidationResults.ValidationErrors)
                {
                    var record = csvRecords.First(x => x.RowNumber == validationError.RowNumber);
                    await AddValidationError(bulkUploadValidationErrors, validationError, record);
                }
            }
        }

        private async Task AddValidationError(List<BulkUploadValidationError> bulkUploadValidationErrors, BulkReservationValidation validationError, BulkUploadAddDraftApprenticeshipRequest record)
        {
            var errorToAdd = new Error("ReservationId", validationError.Reason);
            var existingErrorRecord = bulkUploadValidationErrors.FirstOrDefault(x => x.RowNumber == record.RowNumber);
            if (existingErrorRecord != null)
            {
                existingErrorRecord.Errors.Add(errorToAdd);
            }
            else
            {
                bulkUploadValidationErrors.Add(new BulkUploadValidationError(record.RowNumber,
                            await GetEmployerName(record.AgreementId),
                            record.Uln,
                            record.FirstName + " " + record.LastName,
                            new List<Error> { errorToAdd }));
            }
        }
    }
}
