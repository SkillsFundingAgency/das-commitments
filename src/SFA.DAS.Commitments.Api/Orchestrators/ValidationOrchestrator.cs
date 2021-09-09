using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Types.Validation;
using SFA.DAS.Commitments.Application.Commands.ValidateReservation;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

using ApprenticeshipOverlapValidationRequest = SFA.DAS.Commitments.Api.Types.Validation.ApprenticeshipOverlapValidationRequest;
using ValidationFailReason = SFA.DAS.Commitments.Api.Types.Validation.Types.ValidationFailReason;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public class ValidationOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly ICommitmentsLogger _logger;

        public ValidationOrchestrator(IMediator mediator, ICommitmentsLogger logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<IEnumerable<ApprenticeshipOverlapValidationResult>> ValidateOverlappingApprenticeships(
            IEnumerable<ApprenticeshipOverlapValidationRequest> apprenticeshipOverlapValidationRequests)
        {
            var requests = apprenticeshipOverlapValidationRequests.ToList();

            _logger.Trace($"Validating {requests.Count} overlapping validation requests");

            var command = new Application.Queries.GetOverlappingApprenticeships.GetOverlappingApprenticeshipsRequest
                {
                    OverlappingApprenticeshipRequests = requests.Select(Map).ToList()
                };

            var response = await _mediator.SendAsync(command);

            var result = new List<ApprenticeshipOverlapValidationResult>();

            var requestGroups = response.Data.GroupBy(x => x.RequestApprenticeshipId).ToList();

            foreach (var group in requestGroups)
            {
                result.Add(new ApprenticeshipOverlapValidationResult
                {
                    Self = requests.Single(x=> x.ApprenticeshipId == group.Key),
                    OverlappingApprenticeships = 
                        response.Data
                        .Select(MapFrom)
                        .Where(x=> x.RequestApprenticeshipId == group.Key)
                });
            }

            _logger.Info($"Validated {requests.Count} overlapping validation requests");

            return result;
        }

        public async Task<IEnumerable<ApprenticeshipEmailOverlapValidationResult>> ValidateEmailOverlappingApprenticeships(IEnumerable<SFA.DAS.Commitments.Api.Types.Validation.ApprenticeshipEmailOverlapValidationRequest> apprenticeshipEmailOverlapValidationRequest)
        {
            var requests = apprenticeshipEmailOverlapValidationRequest.ToList();

            var command = new Application.Queries.GetEmailOverlappingApprenticeships.GetEmailOverlappingApprenticeshipsRequest
            {
                OverlappingEmailApprenticeshipRequests = requests.Select(MapEmailOverlap).ToList()
            };

            var response = await _mediator.SendAsync(command);

            var result = new List<ApprenticeshipEmailOverlapValidationResult>();

            var requestGroupsEmail = response.Data.GroupBy(x => x.Id).ToList();

            foreach (var group in requestGroupsEmail)
            {
                result.Add(new ApprenticeshipEmailOverlapValidationResult
                {
                    Self = requests.Single(x => x.ApprenticeshipId == group.Key),
                    OverlappingApprenticeships =
                        response.Data
                        .Select(MapOverlappingEmail)
                        .Where(x => x.RequestApprenticeshipId == group.Key)
                });
            }

            _logger.Info($"Validated {requests.Count} email overlapping validation requests");

            return result;
        }

        private OverlappingApprenticeship MapOverlappingEmail(OverlappingEmail source)
        {
            var result = new OverlappingApprenticeship
            {
                Apprenticeship = new Api.Types.Apprenticeship.Apprenticeship
                {
                    Id = (long)source.Id,
                    StartDate = source.StartDate,
                    EndDate = source.EndDate,
                    Email = source.Email,
                    FirstName = source.FirstName,
                    LastName = source.LastName,
                    CommitmentId = (long)source.CohortId,
                    DateOfBirth = source.DateOfBirth
                },
                ValidationFailReason = (ValidationFailReason)source.OverlapStatus,
                RequestApprenticeshipId = source.Id
            };

            return result;
        }

        private Domain.Entities.ApprenticeshipOverlapValidationRequest Map(ApprenticeshipOverlapValidationRequest requests)
        {
            return new Domain.Entities.ApprenticeshipOverlapValidationRequest
            {
                ApprenticeshipId = requests.ApprenticeshipId,
                EndDate = requests.EndDate,
                StartDate = requests.StartDate,
                Uln = requests.Uln
            };
        }

        private Domain.Entities.ApprenticeshipEmailOverlapValidationRequest MapEmailOverlap(SFA.DAS.Commitments.Api.Types.Validation.ApprenticeshipEmailOverlapValidationRequest requests)
        {
            return new Domain.Entities.ApprenticeshipEmailOverlapValidationRequest
            {
                ApprenticeshipId = requests.ApprenticeshipId,
                EndDate = requests.EndDate,
                StartDate = requests.StartDate,
                Email = requests.Email
            };
        }

        private OverlappingApprenticeship MapFrom(ApprenticeshipResult source)
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
                RequestApprenticeshipId = source.RequestApprenticeshipId,
                EmployerAccountId = source.EmployerAccountId,
                LegalEntityName = source.LegalEntityName,
                ProviderId = source.ProviderId,
                ProviderName = source.ProviderName,
                ValidationFailReason = (ValidationFailReason)source.ValidationFailReason,
            };

            return result;
        }
    }
}