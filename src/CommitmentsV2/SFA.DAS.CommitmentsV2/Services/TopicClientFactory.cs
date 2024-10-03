using Microsoft.Azure.ServiceBus;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services;

public class TopicClientFactory : ITopicClientFactory
{
    public ITopicClient CreateClient(string connectionString, string messageGroupName)
    {
        return new TopicClient(connectionString, messageGroupName);
    }
}