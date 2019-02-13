using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Commitments.EFCoreTester.Config
{
    public class WriteConfig
    {
        public int DraftCount { get; set; }
        public int ConfirmedCount { get; set; }
        public bool SingleApprenticeshipPerCommitment { get; set; }
    }
}
