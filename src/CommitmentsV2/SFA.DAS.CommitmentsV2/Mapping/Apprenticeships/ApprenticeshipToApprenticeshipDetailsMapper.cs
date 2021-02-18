using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping.Apprenticeships
{
    public class
        ApprenticeshipToApprenticeshipDetailsMapper : IMapper<Apprenticeship,
            GetApprenticeshipsQueryResult.ApprenticeshipDetails>
    {
        private readonly ICurrentDateTime _currentDateTime;
        private readonly ILogger<ApprenticeshipToApprenticeshipDetailsMapper> _logger;

        public ApprenticeshipToApprenticeshipDetailsMapper(ICurrentDateTime currentDateTime, ILogger<ApprenticeshipToApprenticeshipDetailsMapper> logger)
        {
            _currentDateTime = currentDateTime;
            _logger = logger;
        }

        public Task<GetApprenticeshipsQueryResult.ApprenticeshipDetails> Map(Apprenticeship source)
        {
            LogTheObject(source);
            return Task.FromResult(new GetApprenticeshipsQueryResult.ApprenticeshipDetails
            {
                Id = source.Id,
                FirstName = source.FirstName,
                LastName = source.LastName,
                CourseName = source.CourseName,
                EmployerName = source.Cohort.AccountLegalEntity.Name,
                ProviderName = source.Cohort.Provider.Name,
                StartDate = source.StartDate.GetValueOrDefault(),
                EndDate = source.EndDate.GetValueOrDefault(),
                PauseDate = source.PauseDate.GetValueOrDefault(),
                EmployerRef = source.EmployerRef,
                ProviderRef = source.ProviderRef,
                CohortReference = source.Cohort.Reference,
                DateOfBirth = source.DateOfBirth.GetValueOrDefault(),
                PaymentStatus = source.PaymentStatus,
                ApprenticeshipStatus = source.MapApprenticeshipStatus(_currentDateTime),
                TotalAgreedPrice = source.PriceHistory.GetPrice(_currentDateTime.UtcNow),
                Uln = source.Uln,
                Alerts = source.MapAlerts(),
                AccountLegalEntityId = source.Cohort.AccountLegalEntityId,
                ProviderId = source.Cohort.ProviderId
            });
        }

        private void LogTheObject(Apprenticeship request)
        {
            try
            {
                _logger.LogDebug("and apprenticeship Id is " + request.Id);
                _logger.LogDebug("and commitment Id is " + request.CommitmentId);
                if (request.PriceHistory?.Count > 0)
                {
                    _logger.LogDebug("price history found");
                    foreach (var pricehistory in request.PriceHistory)
                    {
                        _logger.LogDebug("and pricehistory Id is " + pricehistory.Id);
                        _logger.LogDebug("and pricehistory fromDate " + pricehistory.FromDate);
                        _logger.LogDebug("and pricehistory toDate " + pricehistory.ToDate);
                    }
                }
                else
                {
                    _logger.LogDebug("no price history found");
                }
            }
            catch (Exception exc)
            {
                _logger.LogDebug("unabel to convert to json " + exc.Message);
                _logger.LogDebug("and apprenticeship Id is " + request.Id);
                _logger.LogDebug("and commitment Id is " + request.CommitmentId);
            }
        }

    }
}