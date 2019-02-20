using SFA.DAS.NServiceBus;

namespace SFA.DAS.EmployerAccounts.Messages.Events
{
    public class HealthCheckEvent : Event
    {
        public int Id { get; set; }
    }
}