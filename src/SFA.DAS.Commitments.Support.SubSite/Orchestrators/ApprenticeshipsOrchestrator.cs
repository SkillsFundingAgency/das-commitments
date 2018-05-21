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
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
using SFA.DAS.HashingService;

namespace SFA.DAS.Commitments.Support.SubSite.Orchestrators
{
    public class ApprenticeshipsOrchestrator : IApprenticeshipsOrchestrator
    {
        private readonly ILog _logger;
        private readonly IMediator _mediator;
        private readonly IValidator<ApprenticeshipSearchQuery> _searchValidator;
        private readonly IHashingService _hashingService;
        private readonly IApprenticeshipMapper _apprenticeshipMapper;

        public ApprenticeshipsOrchestrator(ILog logger,
                                            IMediator mediator,
                                            IApprenticeshipMapper apprenticeshipMapper,
                                            IValidator<ApprenticeshipSearchQuery> searchValidator,
                                            IHashingService hashingService)
        {
            _logger = logger ?? throw new ArgumentException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentException(nameof(mediator));
            _searchValidator = searchValidator ?? throw new ArgumentException(nameof(searchValidator));
            _hashingService = hashingService ?? throw new ArgumentException(nameof(hashingService));
            _apprenticeshipMapper = apprenticeshipMapper ?? throw new ArgumentException(nameof(apprenticeshipMapper));
        }

        public async Task<ApprenticeshipViewModel> GetApprenticeship(string hashId, string accountHashedId)
        {
            _logger.Trace("Retrieving Apprenticeship Details");


            var apprenticeshipId = _hashingService.DecodeValue(hashId);
            var accountId = _hashingService.DecodeValue(accountHashedId);

            var response = await _mediator.SendAsync(new GetApprenticeshipRequest
            {
                Caller = new Caller
                {
                    Id = accountId,
                    CallerType = CallerType.Support
                },
                ApprenticeshipId = apprenticeshipId
            });

            if(response == null)
            {
                var errorMsg = $"Can't find Apprenticeship with Hash Id {hashId}";
                _logger.Warn(errorMsg);

                throw new Exception(errorMsg);
            }

            return _apprenticeshipMapper.MapToApprenticeshipViewModel(response.Data);
        }

        public async Task<UlnSearchResultSummaryViewModel> GetApprenticeshipsByUln(ApprenticeshipSearchQuery searchQuery)
        {
            _logger.Trace("Retrieving Apprenticeships Record");

            var validationResult = _searchValidator.Validate(searchQuery);

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

            if (response?.TotalCount == 0)
            {
                return new UlnSearchResultSummaryViewModel
                {
                    ReponseMessages = { "No record Found" }
                };
            }

            _logger.Info($"Apprenticeships Record Count: {response.TotalCount}");

            return _apprenticeshipMapper.MapToUlnResultView(response);
        }


    }
}
