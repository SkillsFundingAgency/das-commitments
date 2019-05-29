using SFA.DAS.Commitments.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Application.Services
{
    public class EventConsumer : IEventConsumer
    {
        private readonly Dictionary<Type, List<Func<object, Task>>> _routes = new Dictionary<Type, List<Func<object, Task>>>();

        public Task Consume<T>(T message) where T : class
        {
            List<Func<object, Task>> handlers;
            if (!_routes.TryGetValue(message.GetType(), out handlers))
            {
                return Task.CompletedTask;
            }
            return Task.WhenAll(handlers.Select(handler => handler(message)));
        }

        public void RegisterHandler<T>(Func<T, Task> handler) where T : class
        {
            List<Func<object, Task>> handlers;
            if (!_routes.TryGetValue(typeof(T), out handlers))
            {
                handlers = new List<Func<object, Task>>();
                _routes.Add(typeof(T), handlers);
            }
            handlers.Add(x => handler((T)x));
        }
    }
}
