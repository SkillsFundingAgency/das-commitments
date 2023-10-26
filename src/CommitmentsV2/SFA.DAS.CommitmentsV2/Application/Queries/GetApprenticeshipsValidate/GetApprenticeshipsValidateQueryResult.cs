using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsValidate
{
    public class GetApprenticeshipsValidateQueryResult
    {
        public IEnumerable<ApprenticeshipValidateModel> Apprenticeships { get; set; }
    }
}
