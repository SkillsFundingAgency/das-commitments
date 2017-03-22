using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

using SFA.DAS.Commitments.Api.Types.Validation;
using SFA.DAS.Commitments.Api.Types.Validation.Types;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships
{
    public sealed class GetOverlappingApprenticeshipsQueryHandler : IAsyncRequestHandler<GetOverlappingApprenticeshipsRequest, GetOverlappingApprenticeshipsResponse>
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly IApprenticeshipOverlapRules _overlapRules;
        private readonly ICommitmentsLogger _logger;
        private readonly AbstractValidator<GetOverlappingApprenticeshipsRequest> _validator;

        public GetOverlappingApprenticeshipsQueryHandler(IApprenticeshipRepository apprenticeshipRepository, AbstractValidator<GetOverlappingApprenticeshipsRequest> validator, IApprenticeshipOverlapRules overlapRules, ICommitmentsLogger logger)
        {
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
            _overlapRules = overlapRules;
            _logger = logger;
        }

        public async Task<GetOverlappingApprenticeshipsResponse> Handle(GetOverlappingApprenticeshipsRequest query)
        {
            var validationResult = _validator.Validate(query);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var result = new GetOverlappingApprenticeshipsResponse
            {
                Data = new List<OverlappingApprenticeship>()
            };

            if (!query.OverlappingApprenticeshipRequests.Any())
                return result;

            var ulns = query.OverlappingApprenticeshipRequests.Select(x => x.Uln);

            var apprenticeships = await _apprenticeshipRepository.GetActiveApprenticeshipsByUlns(ulns);

            foreach (var request in query.OverlappingApprenticeshipRequests)
            {
                foreach (var apprenticeship in apprenticeships)
                {
                    var validationFailReason = _overlapRules.DetermineOverlap(request, apprenticeship);

                    if (validationFailReason != ValidationFailReason.None)
                    {
                        _logger.Info($"ULN: {request.Uln} {request.StartDate:MMM yyyy} - {request.EndDate:MMM yyyy} Reason: {validationFailReason} " +
                                     $"with Apprenticeship Id: {apprenticeship.Id} {apprenticeship.StartDate:MMM yyyy} - {apprenticeship.EndDate:MMM yyyy}");
                        result.Data.Add(MapFrom(apprenticeship, validationFailReason));
                    }
                }
            }

            return result;
        }

        private OverlappingApprenticeship MapFrom(ApprenticeshipResult source, ValidationFailReason validationFailReason)
        {
            var result = new OverlappingApprenticeship
            {
                Apprenticeship = new Api.Types.Apprenticeship.Apprenticeship
                {
                    Id = source.Id,
                    CommitmentId = source.CommitmentId,
                    StartDate = source.StartDate,
                    EndDate = source.EndDate,
                    ULN = source.Uln,
                    EmployerAccountId = source.EmployerAccountId,
                    ProviderId = source.ProviderId,
                    TrainingType = (Api.Types.Apprenticeship.Types.TrainingType)source.TrainingType,
                    TrainingCode = source.TrainingCode,
                    TrainingName = source.TrainingName,
                    Cost = source.Cost,
                    PaymentStatus = (Api.Types.Apprenticeship.Types.PaymentStatus)source.PaymentStatus,
                    AgreementStatus = (Api.Types.AgreementStatus)source.AgreementStatus,
                    DateOfBirth = source.DateOfBirth,
                    EmployerRef = source.EmployerRef,
                    ProviderRef = source.ProviderRef,
                    FirstName = source.FirstName,
                    LastName = source.LastName
                },
                EmployerAccountId = source.EmployerAccountId,
                LegalEntityName = source.LegalEntityName,
                ProviderId = source.ProviderId,
                ProviderName = source.ProviderName,
                ValidationFailReason = validationFailReason,
            };

            return result;
        }

    }
}
