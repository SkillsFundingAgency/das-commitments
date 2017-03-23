using System;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Queries.GetPendingApprenticeshipUpdate
{
    public class GetPendingApprenticeshipUpdateResponse: QueryResponse<PendingApprenticeshipUpdatePlaceholder>
    {
    }

    //todo: replace with API type
    public class PendingApprenticeshipUpdatePlaceholder
    {
        public long Id { get; set; }
        public long ApprenticeshipId { get; set; }
        public CallerType Originator { get; set; }
        public ApprenticeshipUpdateStatus Status { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string ULN { get; set; }
        public TrainingType? TrainingType { get; set; }
        public string TrainingCode { get; set; }
        public string TrainingName { get; set; }
        public decimal? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    //todo: replace with API type
    public enum ApprenticeshipUpdateStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }

}
