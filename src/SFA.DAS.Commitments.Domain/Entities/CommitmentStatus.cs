using System;

namespace SFA.DAS.Commitments.Domain.Entities
{
    public enum CommitmentStatus : short
    {
        New = 0,    // not yet sent to the other party
        Active = 1, // has been sent to the other party
        Deleted = 2
    }
}
