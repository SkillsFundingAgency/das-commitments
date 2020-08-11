using SFA.DAS.CommitmentsV2.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IApprenticeshipDomainService
    {
        Task<Apprenticeship> GetApprenticeshipById(long apprenticeshipId);
    }
}
