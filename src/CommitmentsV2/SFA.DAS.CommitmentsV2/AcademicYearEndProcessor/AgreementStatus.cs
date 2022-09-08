using System.ComponentModel;

namespace SFA.DAS.CommitmentsV2.Domain.Entities
{
    /// <summary>
    /// Indicates whether the apprenticeship has been approved by neither, either, or both parties.
    /// (Individual apprenticeships within a commitment can have different statuses,
    /// since new records can be added to a previously approved commitment.)
    /// </summary>
    public enum AgreementStatus : short
    {
        [Description("Not agreed")]
        NotAgreed = 0,
        [Description("Employer agreed")]
        EmployerAgreed = 1,
        [Description("Provider agreed")]
        ProviderAgreed = 2,
        [Description("Both agreed")]
        BothAgreed = 3
    }
}
