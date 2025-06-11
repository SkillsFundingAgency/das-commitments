using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models;

public interface ICohortOriginator
{
    //Ideally, this method would have only the Id of the other party instead of Provider and AccountLegalEntity, but we need the extra properties that these
    //provide to be backwards-compatible with the de-normalised v1.
    Cohort CreateCohort(long providerId,
        AccountLegalEntity accountLegalEntity,
        Account transferSender,
        int? pledgeApplicationId,
        DraftApprenticeshipDetails draftApprenticeshipDetails,
        UserInfo userInfo,
        int maximumAgeAtApprenticeshipStart
    );

    Cohort CreateCohort(long providerId,
        AccountLegalEntity accountLegalEntity,
        UserInfo userInfo);
}