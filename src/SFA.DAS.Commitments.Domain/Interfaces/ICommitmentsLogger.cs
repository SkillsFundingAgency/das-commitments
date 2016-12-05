using System;

namespace SFA.DAS.Commitments.Domain.Interfaces
{
    public interface ICommitmentsLogger
    {
        void Trace(string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), string lastAction = null, string paymentStatus = null);
        void Debug(string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), string lastAction = null, string paymentStatus = null);
        void Info(string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), string lastAction = null, string paymentStatus = null);
        void Warn(string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), string lastAction = null, string paymentStatus = null);
        void Warn(Exception ex, string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), string lastAction = null, string paymentStatus = null);
        void Error(Exception ex, string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), string lastAction = null, string paymentStatus = null);
        void Fatal(Exception ex, string message, long? accountId = default(long?), long? providerId = default(long?), long? commitmentId = default(long?), long? apprenticeshipId = default(long?), string lastAction = null, string paymentStatus = null);
    }
}
