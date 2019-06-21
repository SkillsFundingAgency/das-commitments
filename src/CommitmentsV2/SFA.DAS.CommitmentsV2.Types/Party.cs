﻿using System;

namespace SFA.DAS.CommitmentsV2.Types
{
    [Flags]
    public enum Party : short
    {
        None = 0,
        Employer = 1,
        Provider = 2,
        TransferSender = 4
    }
}
