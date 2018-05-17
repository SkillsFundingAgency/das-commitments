using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Notifications.Api.Types;

namespace SFA.DAS.Commitments.Notification.WebJob.EmailServices
{
    public class SendingEmployerTransferRequestEmailService : ISendingEmployerTransferRequestEmailService
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IAccountApiClient _accountApi;
        private readonly ILog _logger;
        private RetryPolicy _retryPolicy;

        public SendingEmployerTransferRequestEmailService(ICommitmentRepository commitmentRepository,
            IAccountApiClient accountApi,
            ILog logger)
        {
            _commitmentRepository = commitmentRepository;
            _accountApi = accountApi;
            _logger = logger;
            _retryPolicy = GetRetryPolicy();
        }

        public Task<IEnumerable<Email>> GetEmails()
        {
            throw new NotImplementedException();
        }

        private RetryPolicy GetRetryPolicy()
        {
            return Policy
                .Handle<Exception>()
                .RetryAsync(3,
                    (exception, retryCount) =>
                    {
                        _logger.Warn($"Error connecting to EAS Account Api: ({exception.Message}). Retrying...attempt {retryCount})");
                    }
                );
        }
    }
}
