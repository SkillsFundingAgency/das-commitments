using System;

namespace SFA.DAS.CommitmentsV2.Models
{
    public partial class Provider
    {
        public long UkPrn { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
