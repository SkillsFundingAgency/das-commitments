using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using System;

namespace SFA.DAS.CommitmentsV2.Domain.Data
{
    public interface IApprenticeshipRepository
    {
        Task<ApprenticeshipDetails> GetApprenticeship(long apprenticeshipId);
    }
}