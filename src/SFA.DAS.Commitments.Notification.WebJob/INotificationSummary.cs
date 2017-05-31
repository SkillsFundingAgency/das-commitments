using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Notification.WebJob
{
    internal interface INotificationSummary
    {
        Task GenerateSummary();
    }

    internal class NotificationSummary : INotificationSummary
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;

        public NotificationSummary(IApprenticeshipRepository apprenticeshipRepository)
        {
            _apprenticeshipRepository = apprenticeshipRepository;
        }

        public async Task GenerateSummary()
        {
            var alertsummaries = await _apprenticeshipRepository.GetEmployerApprenticeshipAlertSummary();

            var employerIds = alertsummaries
                .Select(m => m.EmployerAccountId)
                .Distinct();

            foreach (var employerId in employerIds)
            {
                // GetEmailUsers
                var apprenticeships = _apprenticeshipRepository.GetApprenticeshipsByEmployer(employerId);
                var summaryModel = Map(apprenticeships);
            }
        }

        private SummayModel Map(Task<IList<Apprenticeship>> apprenticeships)
        {
            throw new System.NotImplementedException();
        }
    }

    internal class SummayModel
    {
        public long AccountId { get; set; }

        public int TotalCount { get; set; }

        public int ChangesForReviewCount { get; set; }

        public int RequestedChangesCount { get; set; }
    }
}