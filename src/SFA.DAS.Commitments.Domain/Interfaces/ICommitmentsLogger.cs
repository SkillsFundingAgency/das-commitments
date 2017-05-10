using System;

namespace SFA.DAS.Commitments.Domain.Interfaces
{
    public interface ICommitmentsLogger
    {
        void Trace(string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), int? recordCount = default(int?));
        void Debug(string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), int? recordCount = default(int?));
        void Info(string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), int? recordCount = default(int?));
        void Warn(string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), int? recordCount = default(int?));
        void Warn(Exception ex, string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), int? recordCount = default(int?));
        void Error(Exception ex, string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), int? recordCount = default(int?));
        void Fatal(Exception ex, string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), int? recordCount = default(int?));
    }
}
