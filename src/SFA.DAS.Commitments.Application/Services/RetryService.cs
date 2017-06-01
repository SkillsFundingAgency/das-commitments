using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Services
{
    public class RetryService
    {
        private readonly ICommitmentsLogger _logger;

        public int RetryWaitTimeInSeconds { get; set; } = 3;

        public RetryService(ICommitmentsLogger logger)
        {
            _logger = logger;
        }

        public async Task<T> Retry<T>(int retryCount, Func<Task<T>> action)
        {
            var exception = new List<Exception>();
            do
            {
                --retryCount;
                try
                {
                    return await action.Invoke();
                }
                catch (Exception ex)
                {
                    exception.Add(ex);
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(RetryWaitTimeInSeconds));
                }
            }
            while (retryCount > 0);

            _logger.Warn(exception.FirstOrDefault(), $"Not able to call service, tried {exception.Count} times");
            return default(T);
        }
    }
}
