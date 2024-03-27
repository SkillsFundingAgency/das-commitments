using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class LegacyTopicMessagePublisher : ILegacyTopicMessagePublisher
    {
        private readonly ITopicClientFactory _topicClientFactory;
        private readonly ILogger<LegacyTopicMessagePublisher> _logger;
        private readonly string _connectionString;

        public LegacyTopicMessagePublisher(ITopicClientFactory topicClientFactory, ILogger<LegacyTopicMessagePublisher> logger, string connectionString)
        {
            _topicClientFactory = topicClientFactory;
            _logger = logger;
            _connectionString = connectionString;
        }

        public async Task PublishAsync<T>(T @event)
        {
            string messageGroupName = GetMessageGroupName(@event);
            ITopicClient client = null;
            try
            {
                client = _topicClientFactory.CreateClient(_connectionString, messageGroupName);
                var messageBody = Serialize(@event);
                Message message = new Message(messageBody);
                await client.SendAsync(message);
                
                _logger.LogInformation($"Sent Message {typeof(T).Name} to Azure ServiceBus ");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error sending Message {typeof(T).Name} to Azure ServiceBus");
                throw;
            }
            finally
            {
                if (client != null && !client.IsClosedOrClosing)
                {
                    this._logger.LogDebug("Closing legacy topic message publisher");
                    await client.CloseAsync();
                }
            }
        }

        private static string GetMessageGroupName(object obj)
        {
            CustomAttributeData customAttributeData = obj.GetType().CustomAttributes.FirstOrDefault<CustomAttributeData>((Func<CustomAttributeData, bool>)(att => att.AttributeType.Name == "MessageGroupAttribute"));
            string str = customAttributeData != null ? (string)customAttributeData.ConstructorArguments.FirstOrDefault<CustomAttributeTypedArgument>().Value : (string)(object)null;
            if (!string.IsNullOrEmpty(str))
                return str;
            return obj.GetType().Name;
        }

        private static byte[] Serialize<T>(T obj)
        {
            var serializer = new DataContractSerializer(typeof(T));
            var stream = new MemoryStream();
            using (var writer = XmlDictionaryWriter.CreateBinaryWriter(stream))
            {
                serializer.WriteObject(writer, obj);
            }
            return stream.ToArray();
        }
    }
}