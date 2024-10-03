namespace SFA.DAS.CommitmentsV2.Configuration;

public class CommitmentsV2Configuration
{
    public string DatabaseConnectionString { get; set; }
    public string MessageServiceBusConnectionString { get; set; }
    public string RedisConnectionString { get; set; }
    public string ZenDeskEmailAddress { get; set; }

    public EventsApiClientConfiguration EventsApi { get; set; }
    public ApprenticeshipInfoServiceConfiguration ApprenticeshipInfoService { get; set; }

    public NServiceBusConfiguration NServiceBusConfiguration { get; set; }

    public AzureActiveDirectoryApiConfiguration AzureADApiAuthentication { get; set; }

    public LevyTransferMatchingApiConfiguration LevyTransferMatchingInnerApiConfiguration { get; set; }

    public ProviderUrlConfiguration ProviderUrlConfiguration { get; set; }

    /// <summary>
    /// An ISO-formatted string date representation for test override, or any other non-empty value for the real current datetime
    /// </summary>
    public string CurrentDateTime { get; set; }
    public string ReadOnlyDatabaseConnectionString { get; set; }

    public string ProviderCommitmentsBaseUrl { get; set; }
    public string EmployerCommitmentsBaseUrl { get; set; }
}