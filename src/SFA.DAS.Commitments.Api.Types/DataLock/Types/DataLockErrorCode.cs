using System;
using System.ComponentModel;

namespace SFA.DAS.Commitments.Api.Types.DataLock.Types
{
    // https://skillsfundingagency.atlassian.net/wiki/spaces/DAS/pages/104941512/Processing+ILR+submissions+and+data+locks
    [Flags]
    public enum DataLockErrorCode
    {
        None = 0,
        [Description("No matching record found in an employer digital account for the UKPRN")]
        Dlock01 = 1,
        [Description("No matching record found in the employer digital account for the ULN")]
        Dlock02 = 2,
        [Description("No matching record found in the employer digital account for the standard code")]
        Dlock03 = 4,
        [Description("No matching record found in the employer digital account for the framework code")]
        Dlock04 = 8,
        [Description("No matching record found in the employer digital account for the programme type")]
        Dlock05 = 16,
        [Description("No matching record found in the employer digital account for the pathway code")]
        Dlock06 = 32,
        [Description("No matching record found in the employer digital account for the negotiated cost of training")]
        Dlock07 = 64,
        [Description("Multiple matching records found in the employer digital account")]
        Dlock08 = 128,
        [Description("The start date for this negotiated price is before the corresponding price start date in the employer digital account")]
        Dlock09 = 256,
        [Description("The employer has stopped payments for this apprentice")]
        Dlock10 = 512
    }
}
