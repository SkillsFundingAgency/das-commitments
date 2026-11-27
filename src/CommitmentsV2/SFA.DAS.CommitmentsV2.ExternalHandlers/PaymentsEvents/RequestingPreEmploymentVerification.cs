using System;

namespace SFA.DAS.Payments.External.Messages.Events.HiringGrant;

// Replace with SFA.DAS.Payments.External.Messages.Events when the package is published.
public class RequestingPreEmploymentVerification
{
    public Guid EventId { get; set; }
    public long? UKPRN { get; set; }
    public long? ULN { get; set; }
    public long? EmployerAccountIdentifier { get; set; }
    public long ApprenticeshipId { get; set; }
    public DateTime RequestTime { get; set; }
}
