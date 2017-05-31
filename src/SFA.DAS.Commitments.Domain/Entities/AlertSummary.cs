namespace SFA.DAS.Commitments.Domain.Entities
{
    public class AlertSummary
    {
        public long EmployerAccountId { get; set; }

        public string LegalEntityId { get; set; }

        public string LegalEntityName { get; set; }

        public int TotalCount { get; set; }

        public int ChangeOfCircCount { get; set; }

        public int RestartRequestCount { get; set; }
    }
}