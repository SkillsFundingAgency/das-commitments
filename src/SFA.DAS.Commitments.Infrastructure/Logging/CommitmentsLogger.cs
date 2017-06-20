using System;
using System.Collections.Generic;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Infrastructure.Logging
{
    public sealed class CommitmentsLogger : ICommitmentsLogger
    {
        private readonly ILog _logger;

        public ILog BaseLogger => _logger;

        public CommitmentsLogger(ILog logger)
        {
            _logger = logger;
        }

        public void Trace(string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), int? recordCount = default(int?))
        {
            IDictionary<string, object> properties = BuildPropertyDictionary(accountId, providerId, commitmentId, apprenticeshipId, recordCount);
            _logger.Trace(message, properties);
        }

        public void Debug(string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), int? recordCount = default(int?))
        {
            IDictionary<string, object> properties = BuildPropertyDictionary(accountId, providerId, commitmentId, apprenticeshipId, recordCount);
            _logger.Debug(message, properties);
        }

        public void Info(string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), int? recordCount = default(int?))
        {
            IDictionary<string, object> properties = BuildPropertyDictionary(accountId, providerId, commitmentId, apprenticeshipId, recordCount);
            _logger.Info(message, properties);
        }

        public void Warn(string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), int? recordCount = default(int?))
        {
            IDictionary<string, object> properties = BuildPropertyDictionary(accountId, providerId, commitmentId, apprenticeshipId, recordCount);
            _logger.Warn(message, properties);
        }

        public void Warn(Exception ex, string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), int? recordCount = default(int?))
        {
            IDictionary<string, object> properties = BuildPropertyDictionary(accountId, providerId, commitmentId, apprenticeshipId, recordCount);
            _logger.Warn(ex, message, properties);
        }

        public void Error(Exception ex, string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), int? recordCount = default(int?))
        {
            IDictionary<string, object> properties = BuildPropertyDictionary(accountId, providerId, commitmentId, apprenticeshipId, recordCount);
            _logger.Error(ex, message, properties);
        }

        public void Fatal(Exception ex, string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), int? recordCount = default(int?))
        {
            IDictionary<string, object> properties = BuildPropertyDictionary(accountId, providerId, commitmentId, apprenticeshipId, recordCount);
            _logger.Fatal(ex, message, properties);
        }

        private IDictionary<string, object> BuildPropertyDictionary(long? accountId, long? providerId, long? commitmentId, long? apprenticeshipId, int? recordCount)
        {
            var properties = new Dictionary<string, object>();

            if (accountId.HasValue) properties.Add("AccountId", accountId.Value);
            if (providerId.HasValue) properties.Add("ProviderId", providerId.Value);
            if (commitmentId.HasValue) properties.Add("CommitmentId", commitmentId.Value);
            if (apprenticeshipId.HasValue) properties.Add("ApprenticeshipId", apprenticeshipId.Value);
            if (recordCount.HasValue) properties.Add("recordCount", recordCount.Value);

            return properties;
        }
    }
}
