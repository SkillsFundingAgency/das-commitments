using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Api.Types.Validation;
using SFA.DAS.Commitments.Api.Types.Validation.Types;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Rules
{
    public interface IApprenticeshipOverlapRules
    {
        ValidationFailReason DetermineOverlap(ApprenticeshipOverlapValidationRequest request, ApprenticeshipResult apprenticeship);
    }
}
