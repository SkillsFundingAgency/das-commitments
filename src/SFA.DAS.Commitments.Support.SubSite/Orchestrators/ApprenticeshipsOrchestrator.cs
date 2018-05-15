using MediatR;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeshipsByUln;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.NLog.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using SFA.DAS.Commitments.Support.SubSite.Extensions;
using SFA.DAS.Commitments.Domain.Entities;
using FluentValidation;

namespace SFA.DAS.Commitments.Support.SubSite.Orchestrators
{
    public class ApprenticeshipsOrchestrator : IApprenticeshipsOrchestrator
    {
        private readonly ILog _logger;
        private readonly IMediator _mediator;
        private readonly IValidator<ApprenticeshipSearchQuery> _searchValidator;

        public ApprenticeshipsOrchestrator(ILog logger, IMediator mediator, IValidator<ApprenticeshipSearchQuery> searchValidator)
        {
            _logger = logger ?? throw new ArgumentException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentException( nameof(mediator) );
            _searchValidator = searchValidator ?? throw new ArgumentException(nameof(searchValidator));
        }

        public async Task<UlnSearchResultSummaryViewModel> GetApprenticeshipsByUln(ApprenticeshipSearchQuery searchQuery)
        {
            _logger.Trace("Retrieving Apprenticeships Record Count");

            var validationResult =   _searchValidator.Validate(searchQuery);

            if (!validationResult.IsValid)
            {
                return new UlnSearchResultSummaryViewModel
                {
                    ReponseMessages = validationResult.Errors.Select(o => o.ErrorMessage).ToList()
                };
            }

            var response = await _mediator.SendAsync(new GetApprenticeshipsByUlnRequest
            { 
                Uln = searchQuery.SearchTerm
            });

            if(response?.TotalCount == 0)
            {
                return new UlnSearchResultSummaryViewModel
                {
                    ReponseMessages = { "No record Found" }
                };
            }

            _logger.Info($"Apprenticeships Record Count: {response.TotalCount}");

            return new UlnSearchResultSummaryViewModel
            {
                Uln = searchQuery.SearchTerm,
                ApprenticeshipsCount = response.TotalCount,
                SearchResults = response.Apprenticeships.Select(o => new UlnSearchResultViewModel
                {
                    ApprenticeName = $"{o.FirstName} {o.LastName}",
                    EmployerName = o.LegalEntityName,
                    ProviderUkprn = o.ProviderId,
                    TrainingDates = $"{o.StartDate.ToGdsFormatWithSlashSeperator() ?? "-"} to {o.EndDate.ToGdsFormatWithSlashSeperator() ?? "-"}",
                    PaymentStatus = MapPaymentStatus(o.PaymentStatus,o.StartDate)
                }).ToList()
            };

        }

        private string MapPaymentStatus(PaymentStatus paymentStatus, DateTime? startDate)
        {
            var isStartDateInFuture = startDate.HasValue && startDate.Value > new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            switch (paymentStatus)
            {
                case PaymentStatus.PendingApproval:
                    return "Approval needed";
                case PaymentStatus.Active:
                    return
                        isStartDateInFuture ? "Waiting to start" : "Live";
                case PaymentStatus.Paused:
                    return "Paused";
                case PaymentStatus.Withdrawn:
                    return "Stopped";
                case PaymentStatus.Completed:
                    return "Finished";
                case PaymentStatus.Deleted:
                    return "Deleted";
                default:
                    return string.Empty;
            }
        }

    }
}
