﻿using System.ComponentModel;

namespace SFA.DAS.CommitmentsV2.Types
{
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
