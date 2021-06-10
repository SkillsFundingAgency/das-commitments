using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace SFA.DAS.CommitmentsV2.TestHelpers
{
    public static class MockILoggerExtensions
    {
        public static Mock<ILogger<T>> VerifyLogging<T>(this Mock<ILogger<T>> logger, string expectedMessage, LogLevel expectedLogLevel = LogLevel.Debug, Func<Times> times = null)
        {
            times = times ?? Times.Once;

            Func<object, Type, bool> state = (v, t) => v.ToString().CompareTo(expectedMessage) == 0;

            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == expectedLogLevel),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => state(v, t)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), times);

            return logger;
        }

        public static Mock<ILogger<T>> VerifyLogging<T>(this Mock<ILogger<T>> logger, LogLevel expectedLogLevel = LogLevel.Debug, Func<Times> times = null)
        {
            times = times ?? Times.Once;

            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == expectedLogLevel),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), times);

            return logger;
        }
    }
}
