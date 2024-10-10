using Microsoft.Azure.ServiceBus;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface ITopicClientFactory
{
    ITopicClient CreateClient(string connectionString, string messageGroupName);
}