using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Extensions;

public static class DraftApprenticeshipDetailsExtensions
{
    public static DateTime? GetStartDate(this DraftApprenticeshipDetails details)
    {
        return details.StartDate;
    }
}