using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Tasks.Api.Client.Configuration;

namespace SFA.DAS.Commitments.Infrastructure.Configuration
{
    public class CommitmentsApiConfiguration : IConfiguration
    {
        public string DatabaseConnectionString { get; set; }
        public string ServiceBusConnectionString { get; set; }
        public TasksApiClientConfiguration TasksApi { get; set; }
    }

    public class TasksApiClientConfiguration : ITasksApiClientConfiguration
    {
        public string BaseUrl { get; set; }
        public string ClientSecret { get; set; }
    }
}
