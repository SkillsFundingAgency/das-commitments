using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class TopicClientFactory : ITopicClientFactory
    {
        public ITopicClient CreateClient(string connectionString, string messageGroupName)
        {
            return new TopicClient(connectionString, messageGroupName);
        }
    }

    public class LearningTransportTopicClientFactory : ITopicClientFactory
    {
        public ITopicClient CreateClient(string connectionString, string messageGroupName)
        {
            return new LearningTransportTopicClient(messageGroupName);
        }
    }

    public class LearningTransportTopicClient : ITopicClient
    {
        private readonly string _topicName;

        public LearningTransportTopicClient(string topicName)
        {
            _topicName = topicName;
        }

        public bool IsClosedOrClosing { get; set; } = false;

        public Task CloseAsync()
        {
            IsClosedOrClosing = true;
            return Task.CompletedTask;
        }

        public Task SendAsync(Message message)
        {
            Console.WriteLine($"Sending message to {_topicName}");
            Console.WriteLine($"Message: {System.Text.Encoding.UTF8.GetString(message.Body)}");
            return Task.CompletedTask;
        }

        public Task SendAsync(IList<Message> messageList) => throw new NotImplementedException();
        public Task<long> ScheduleMessageAsync(Message message, DateTimeOffset scheduleEnqueueTimeUtc) => throw new NotImplementedException();
        public Task CancelScheduledMessageAsync(long sequenceNumber) => throw new NotImplementedException();
        public void RegisterPlugin(ServiceBusPlugin serviceBusPlugin) => throw new NotImplementedException();
        public void UnregisterPlugin(string serviceBusPluginName) => throw new NotImplementedException();
        public string TopicName => throw new NotImplementedException();
        public string ClientId => throw new NotImplementedException();
        public string Path => throw new NotImplementedException();
        public TimeSpan OperationTimeout { get; set; }
        public ServiceBusConnection ServiceBusConnection => throw new NotImplementedException();
        public bool OwnsConnection => throw new NotImplementedException();
        public IList<ServiceBusPlugin> RegisteredPlugins => throw new NotImplementedException();  
    }
}