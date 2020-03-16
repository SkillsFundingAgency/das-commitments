using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IEmployerProviderIdentifier
    {
        long? EmployerAccountId { get; set; }
        long? ProviderId { get; set; }
    }
}
