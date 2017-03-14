using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Rules
{
    public interface IApprenticeshipOverlapRules
    {
        //todo: return reason
        bool IsOverlap(OverlappingApprenticeshipRequest request, ApprenticeshipResult apprenticeship);
    }
}
