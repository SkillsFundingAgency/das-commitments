using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.Logging
{
    public sealed class CommitmentsLogEntry : ILogEntry
    {
        public long? CommitmentId { get; set; }
        public long? AccountId { get; set; }
        public long? ProviderId { get; set; } 
        public long? ApprenticeshipId { get; set; }
    }
}