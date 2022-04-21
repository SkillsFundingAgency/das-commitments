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
        private async Task ValidateReservation(IEnumerable<BulkUploadAddDraftApprenticeshipRequest> csvRecords, long providerId, List<BulkUploadValidationError> bulkUploadValidationErrors)
        {
            var reservationsToValidate = new List<ReservationRequest>();
            foreach (var res in csvRecords)
            {
                var employerDetails = await GetEmployerDetails(res.AgreementId);
                if (
                    employerDetails.IsLevy.HasValue &&  !employerDetails.IsLevy.Value
                    && employerDetails.IsSigned.HasValue && employerDetails.IsSigned.Value 
                    && employerDetails.HasPermissionToCreateCohort.HasValue && employerDetails.HasPermissionToCreateCohort.Value)
                {
                    var reservation = Map(res, employerDetails);
                    reservationsToValidate.Add(reservation);
                }
            }

            if (reservationsToValidate.Any())
            {
                var errors = await _reservationValidationService.BulkValidate(reservationsToValidate, CancellationToken.None);

                if (errors?.ValidationErrors?.Any() ?? false)
                {
                    foreach (var validationError in errors.ValidationErrors)
                    {
                        var record = csvRecords.First(x => x.RowNumber == validationError.RowNumber);
                        await AddValidationError(bulkUploadValidationErrors, validationError, record);
                    }
                }
            }
        }

        private ReservationRequest Map(BulkUploadAddDraftApprenticeshipRequest draftApprenticeshipRequest, EmployerSummary employerDetails)
        {
            var cohortDetails = GetCohortDetails(draftApprenticeshipRequest.CohortRef);
            long.TryParse(employerDetails.AccountLegalEntityId, out var accountLegalEntityId);

            return new ReservationRequest
            {
                Id = System.Guid.NewGuid(),
                StartDate = draftApprenticeshipRequest.StartDate,
                CourseId = draftApprenticeshipRequest.CourseCode,
                ProviderId = (uint)draftApprenticeshipRequest.ProviderId,
                AccountLegalEntityId = accountLegalEntityId,
                TransferSenderAccountId = cohortDetails?.TransferSenderId,
                UserId = System.Guid.Empty,
            };
        }

        private async Task AddValidationError(List<BulkUploadValidationError> bulkUploadValidationErrors, BulkValidation validationError, BulkUploadAddDraftApprenticeshipRequest record)
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
