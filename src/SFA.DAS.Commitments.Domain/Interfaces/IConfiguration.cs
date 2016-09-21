namespace SFA.DAS.Commitments.Domain.Interfaces
{
    public interface IConfiguration
    {
        string DatabaseConnectionString { get; set; }
        string ServiceBusConnectionString { get; set; }
    }
}