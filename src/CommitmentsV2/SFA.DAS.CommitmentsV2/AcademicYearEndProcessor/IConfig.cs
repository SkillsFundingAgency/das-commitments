namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IConfig
    {
        string DatabaseConnectionString { get; set; }
        string ServiceBusConnectionString { get; set; }
    }
}