namespace SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces
{
    public interface IConfiguration
    {
        string DatabaseConnectionString { get; set; }
        string ServiceBusConnectionString { get; set; }
    }
}