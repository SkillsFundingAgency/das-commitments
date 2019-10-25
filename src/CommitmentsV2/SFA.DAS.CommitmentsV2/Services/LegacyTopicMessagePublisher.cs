using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

        public async Task PublishAsync(object @event)
        {
            string messageGroupName = GetMessageGroupName(@event);
            ITopicClient client = null;
            try
            {
                client = _topicClientFactory.CreateClient(_connectionString, messageGroupName);
                string messageBody = JsonConvert.SerializeObject(@event);
                Message message = new Message(System.Text.Encoding.UTF8.GetBytes(messageBody));
                await client.SendAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error sending Message to Azure ServiceBus");
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
    }
}