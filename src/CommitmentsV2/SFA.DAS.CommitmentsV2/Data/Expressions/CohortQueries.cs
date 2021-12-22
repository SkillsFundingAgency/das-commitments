using System;
using System.Linq.Expressions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Data.Expressions
{
    public static class CohortQueries
    {

        public static Expression<Func<Cohort, bool>> IsFullyApproved()
        {
            return cohort => cohort.EditStatus == EditStatus.Both &&
                             (!cohort.TransferSenderId.HasValue ||
                              cohort.TransferApprovalStatus ==
                              TransferApprovalStatus.Approved);
        }
    }
}