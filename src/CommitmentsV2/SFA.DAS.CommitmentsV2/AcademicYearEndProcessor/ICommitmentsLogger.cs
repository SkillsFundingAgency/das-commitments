using NServiceBus.Logging;
//using SFA.DAS.NLog.Logger;
using System;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface ICommitmentsLogger
    {
        ILog BaseLogger { get; }
        void Trace(string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), int? recordCount = default(int?), Caller caller = null);
        void Debug(string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), int? recordCount = default(int?), Caller caller = null);
        void Info(string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), int? recordCount = default(int?), Caller caller = null);
        void Warn(string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), int? recordCount = default(int?));
        void Warn(Exception ex, string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), int? recordCount = default(int?));
        void Error(Exception ex, string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), int? recordCount = default(int?));
        void Fatal(Exception ex, string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), int? recordCount = default(int?));
    }
}
