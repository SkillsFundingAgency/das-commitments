using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services;

public class LegacyTopicMessagePublisher(ITopicClientFactory topicClientFactory, ILogger<LegacyTopicMessagePublisher> logger, string connectionString)
    : ILegacyTopicMessagePublisher
{
    public async Task PublishAsync<T>(T @event)
    {
        var messageGroupName = GetMessageGroupName(@event);
        ITopicClient client = null;
        try
        {
            client = topicClientFactory.CreateClient(connectionString, messageGroupName);
            var messageBody = Serialize(@event);
            var message = new Message(messageBody);
            await client.SendAsync(message);

            logger.LogInformation("Sent Message {Name} to Azure ServiceBus ", typeof(T).Name);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error sending Message {Name} to Azure ServiceBus", typeof(T).Name);
            throw;
        }
        finally
        {
            if (client != null && !client.IsClosedOrClosing)
            {
                logger.LogDebug("Closing legacy topic message publisher");
                await client.CloseAsync();
            }
        }
    }

    private static string GetMessageGroupName(object obj)
    {
        var customAttributeData = obj.GetType().CustomAttributes.FirstOrDefault((Func<CustomAttributeData, bool>)(att => att.AttributeType.Name == "MessageGroupAttribute"));
        var str = customAttributeData != null
            ? (string)customAttributeData.ConstructorArguments.FirstOrDefault().Value
            : null;

        if (!string.IsNullOrEmpty(str))
        {
            return str;
        }

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