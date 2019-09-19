﻿using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Api.Types.Configuration;

namespace SFA.DAS.Reservations.Api.Types
{
    public class ReservationsApiClient : IReservationsApiClient
    {
        private readonly ReservationsClientApiConfiguration _config;
        private readonly IHttpHelper _httpHelper;

        public ReservationsApiClient(ReservationsClientApiConfiguration config, IHttpHelper httpHelper)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpHelper = httpHelper ?? throw new ArgumentNullException(nameof(httpHelper));
        }

        public Task<ReservationValidationResult> ValidateReservation(ReservationValidationMessage request, CancellationToken cancellationToken)
        {
            var url = BuildUrl($"api/reservations/validate/{request.ReservationId}");

            var data = new
            {
                StartDate = request.StartDate.ToString("yyyy-MM-dd"),
                request.CourseCode
            };

            return _httpHelper.GetAsync<ReservationValidationResult>(url, data, cancellationToken);
        }

        public Task<ReservationAllocationStatusResult> GetReservationAllocationStatus(ReservationAllocationStatusMessage request, CancellationToken cancellationToken)
        {
            var optionQueryString=request.TransferSenderId.HasValue ? $"?transferSenderId={request.TransferSenderId}" : "";

            var url = BuildUrl($"api/accounts/{request.AccountId}/status{optionQueryString}");

            return _httpHelper.GetAsync<ReservationAllocationStatusResult>(url, null, cancellationToken);
        }

        public Task<BulkCreateReservationsResult> BulkCreateReservations(long accountLegalEntityId, BulkCreateReservationsRequest request, CancellationToken cancellationToken)
        {
            var url = BuildUrl($"api/reservations/accounts/{accountLegalEntityId}/bulk-create");
            return _httpHelper.PostAsJson<BulkCreateReservationsRequest, BulkCreateReservationsResult>(url, request, cancellationToken);
        }

        private string BuildUrl(string path)
        {
            var effectiveApiBaseUrl = _config.EffectiveApiBaseUrl.TrimEnd(new[] { '/' });
            path = path.TrimStart(new[] { '/' });

            return $"{effectiveApiBaseUrl}/{path}";
        }
    }
}