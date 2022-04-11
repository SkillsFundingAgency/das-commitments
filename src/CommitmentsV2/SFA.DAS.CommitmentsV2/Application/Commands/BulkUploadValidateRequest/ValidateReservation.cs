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
        private async Task<BulkValidationResults> ValidateReservation(IEnumerable<BulkUploadAddDraftApprenticeshipRequest> csvRecords, long providerId)
        {
            List<ReservationRequest> result = new List<ReservationRequest>();
            foreach (var res in csvRecords)
            {
                var x = await Map(res);
                result.Add(x);
            }

            //IEnumerable<ReservationRequest> result = csvRecords.Select(x => { var result = Map(x).Result; return result; });

            return await _reservationValidationService.BulkValidate(result, CancellationToken.None);
        }

        private async Task<ReservationRequest> Map(BulkUploadAddDraftApprenticeshipRequest x)
        {
            var employerDetails = await GetEmployerDetails(x.AgreementId);
            var cohortDetails = GetCohortDetails(x.CohortRef);

            return new ReservationRequest
            {
                Id = System.Guid.NewGuid(),
                StartDate = x.StartDate,
                CourseId = x.CourseCode,
                ProviderId = (uint)x.ProviderId,
                AccountLegalEntityId = employerDetails.LegalEntityId ?? 0,
                TransferSenderAccountId = cohortDetails?.TransferSenderId,
                UserId = System.Guid.Empty
            };
        }
    }
}
