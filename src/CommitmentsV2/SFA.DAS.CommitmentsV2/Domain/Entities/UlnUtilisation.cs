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

        public long ApprenticeshipId { get; private set; }
        public string Uln { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
    }
}
