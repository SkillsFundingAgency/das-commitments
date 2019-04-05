namespace SFA.DAS.Commitments.Application.Interfaces
{
    public interface INServiceBusConfiguration
    {
        string EndpointName { get; }
        string TransportConnectionString { get; }
        string License { get; }
    }
}