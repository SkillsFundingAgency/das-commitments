using System.ComponentModel;

namespace SFA.DAS.Commitments.Domain.Entities
{
    /// <summary>
    /// Indicates which party the commitment currently sits with for editing.
    /// </summary>
    public enum EditStatus
    {
        [Description("Indicates approval by both employer (receiving employer if transfer) and provider")] // note: badly named, not editable by either!
        Both = 0,
        [Description("Editable by employer")]
        EmployerOnly = 1,
        [Description("Editable by provider")]
        ProviderOnly = 2,
        [Description("Not Used")]
        Neither = 3
    }
}
