using System;

namespace SFA.DAS.CommitmentsV2.TestSubscriber
{
    public static class ExceptionExtensions
    {
        public static void DumpException(this AggregateException exception)
        {
            foreach (var innerException in exception.InnerExceptions)
            {
                innerException.DumpException();
            }
        }

        public static void DumpException(this Exception exception)
        {
            while (exception != null)
            {
                Console.WriteLine($"{exception.GetType().Name}: {exception.Message}");
                exception = exception.InnerException;
            }
        }
    }
}