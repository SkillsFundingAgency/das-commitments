﻿using System.ComponentModel;

namespace SFA.DAS.Commitments.Api.Types.Commitment.Types
{
    /// <summary>
    /// For unapproved apprenticeships, indicates which party the commitment currently sits with for editing.
    /// </summary>
    public enum EditStatus
    {
        [Description("Indicates approval by both employer (receiving employer if transfer) and provider")]
        Both = 0,
        [Description("Editable by employer")]
        EmployerOnly = 1,
        [Description("Editable by provider")]
        ProviderOnly = 2,
        [Description("Not Used")]
        Neither = 3
    }
}
