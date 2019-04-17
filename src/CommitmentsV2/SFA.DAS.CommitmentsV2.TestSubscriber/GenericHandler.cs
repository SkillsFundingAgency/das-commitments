using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.TestSubscriber
{
    class GenericHandler : IHandleMessages<object> 
    {
        public Task Handle(object message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Received message type {message.GetType().FullName} : Content: {JsonConvert.SerializeObject(message)}");
            return Task.CompletedTask;
        }
    }
}
