namespace SFA.DAS.Commitments.Domain.Entities
{
    public class LastUpdateAction
    {
        public LastAction LastAction { get; set; }
        public Caller Caller { get; set; }
        public string LastUpdaterName { get; set; }
        public string LastUpdaterEmail { get; set; }
        public string UserId { get; set; }
    }
}