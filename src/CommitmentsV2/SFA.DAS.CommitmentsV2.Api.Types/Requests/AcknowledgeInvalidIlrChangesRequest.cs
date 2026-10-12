using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class AcknowledgeInvalidIlrChangesRequest : SaveDataRequest
{
    public long ProviderId { get; set; }
    public List<InvalidIlrChangeAcknowledgement> Acknowledgements { get; set; } = [];
}

public class InvalidIlrChangeAcknowledgement
{
    public Guid ApprovalRequestId { get; set; }
    public bool? DeleteAlert { get; set; }
}
