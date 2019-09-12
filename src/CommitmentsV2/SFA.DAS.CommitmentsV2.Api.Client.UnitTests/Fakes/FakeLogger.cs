using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Client.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Api.Client.UnitTests.Fakes
{
    public class LogMessage
    {
        public LogMessage(LogLevel logLevel, string message)
        {
            LogLevel = logLevel;
            Message = message;
        }
        public LogLevel LogLevel { get; }
        public string Message { get; }
    }

    public class FakeLogger : ILogger<CommitmentsRestHttpClient>
    {
        private readonly List<LogLevel> _enabledLogLevels = new List<LogLevel>();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                Messages.Add(new LogMessage(logLevel, formatter(state, exception)));
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _enabledLogLevels.Contains(logLevel);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public void EnableLevel(LogLevel logLevel)
        {
            if (!_enabledLogLevels.Contains(logLevel))
            {
                _enabledLogLevels.Add(logLevel);
            }
        }

        public List<LogMessage> Messages { get; } = new List<LogMessage>();

        public bool ContainsMessage(Func<LogMessage, bool> matches)
        {
            return Messages.Any(matches);
        }
    }
}
