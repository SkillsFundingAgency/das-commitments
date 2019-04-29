using System;

namespace SFA.DAS.CommitmentsV2.Domain.Entities
{
    public class UlnUtilisation
    {
        public UlnUtilisation(long apprenticeshipId, string uln, DateTime startDate, DateTime endDate)
        {
            ApprenticeshipId = apprenticeshipId;
            Uln = uln;
            StartDate = startDate;
            EndDate = endDate;
        }

        public long ApprenticeshipId { get; }
        public string Uln { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
    }
}
