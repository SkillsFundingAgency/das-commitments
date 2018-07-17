using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Services
{
    internal class ApprenticeshipEventsService
    {
        private readonly IApprenticeshipEventsList _apprenticeshipEventsList;
        private readonly IApprenticeshipEventsPublisher _apprenticeshipEventsPublisher;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private ICommitmentsLogger _logger;

        internal ApprenticeshipEventsService(IApprenticeshipEventsList apprenticeshipEventsList, IApprenticeshipEventsPublisher apprenticeshipEventsPublisher, IApprenticeshipRepository apprenticeshipRepository, ICommitmentsLogger logger)
        {
            _apprenticeshipEventsList = apprenticeshipEventsList;
            _apprenticeshipEventsPublisher = apprenticeshipEventsPublisher;
            _apprenticeshipRepository = apprenticeshipRepository;
            _logger = logger;
        }

        internal async Task PublishApprenticeshipAgreementUpdatedEvents(Commitment commitment)
        {
            Parallel.ForEach(commitment.Apprenticeships, apprenticeship =>
            {
                _apprenticeshipEventsList.Add(commitment, apprenticeship, "APPRENTICESHIP-AGREEMENT-UPDATED");
            });
            await _apprenticeshipEventsPublisher.Publish(_apprenticeshipEventsList);
        }

        internal async Task PublishApprenticeshipFinalApprovalEvents(Commitment commitment)
        {
            _logger.Info("Getting active apprenticeships for learners");
            var existingApprenticeships = await GetActiveApprenticeshipsForLearners(commitment.Apprenticeships);

            Parallel.ForEach(commitment.Apprenticeships, apprenticeship =>
            {
                var effectiveFromDate = DetermineApprovalEventEffectiveFromDate(apprenticeship.AgreementStatus, existingApprenticeships, apprenticeship.StartDate);
                _apprenticeshipEventsList.Add(commitment, apprenticeship, "APPRENTICESHIP-AGREEMENT-UPDATED", effectiveFromDate);
            });

            _logger.Info($"Publishing {existingApprenticeships.Count()} apprenticeship agreement updates");
            await _apprenticeshipEventsPublisher.Publish(_apprenticeshipEventsList);
        }

        private async Task<IEnumerable<ApprenticeshipResult>> GetActiveApprenticeshipsForLearners(IList<Apprenticeship> updatedApprenticeships)
        {
            var ulns = updatedApprenticeships.Select(x => x.ULN);
            var apprenticeships = await _apprenticeshipRepository.GetActiveApprenticeshipsByUlns(ulns);
            return apprenticeships;
        }

        private DateTime? DetermineApprovalEventEffectiveFromDate(AgreementStatus agreementStatus, IEnumerable<ApprenticeshipResult> existingApprenticeships, DateTime? startDate)
        {
            var previousApprenticeshipStoppedDate = GetPreviousApprenticeshipStoppedDate(existingApprenticeships, startDate);
            if (HasPreviousApprenticeshipStoppedInTheSameMonth(previousApprenticeshipStoppedDate, startDate))
            {
                return previousApprenticeshipStoppedDate.Value.AddDays(1);
            }

            return new DateTime(startDate.Value.Year, startDate.Value.Month, 1);
        }

        private bool HasPreviousApprenticeshipStoppedInTheSameMonth(DateTime? previousApprenticeshipStoppedDate, DateTime? startDate)
        {
            if (!previousApprenticeshipStoppedDate.HasValue)
            {
                return false;
            }

            if (previousApprenticeshipStoppedDate.Value.Year != startDate.Value.Year || previousApprenticeshipStoppedDate.Value.Month != startDate.Value.Month)
            {
                return false;
            }

            return true;
        }

        private DateTime? GetPreviousApprenticeshipStoppedDate(IEnumerable<ApprenticeshipResult> existingApprenticeships, DateTime? startDate)
        {
            var previousApprenticeships = GetPreviousApprenticeships(existingApprenticeships, startDate.Value);
            if (!previousApprenticeships.Any())
            {
                return null;
            }

            var latestApprenticeship = previousApprenticeships.OrderByDescending(x => x.StartDate).First();
            return latestApprenticeship.StopDate;
        }

        private IEnumerable<ApprenticeshipResult> GetPreviousApprenticeships(IEnumerable<ApprenticeshipResult> existingApprenticeships, DateTime startDate)
        {
            return existingApprenticeships.Where(x => x.StartDate < startDate);
        }
    }
}
