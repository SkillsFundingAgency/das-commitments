using System;

namespace SFA.DAS.Commitments.Domain.Entities
{
    public class EmailToValidate
    {
        public string Email { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public long? ApprenticeshipId { get; }
        public long RowId { get; }

        public EmailToValidate(string email, DateTime startDate, DateTime endDate, long? apprenticeshipId, long rowId)
        {
            Email = email;
            StartDate = startDate;
            EndDate = endDate;
            ApprenticeshipId = apprenticeshipId; //ROWId
            RowId = rowId;

        }
    }
}
