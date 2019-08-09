using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.Commitments.Domain.Entities
{
    public class Account
    {
        public long Id { get; set; }
        public ApprenticeshipEmployerType ApprenticeshipEmployerType { get; set; }
    }
}