using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Extensions
{
    public static class ApprenticeshipExtensions
    {
        public static bool IsWaitingToStart(this Apprenticeship apprenticeship, ICurrentDateTime currentDateTime)
        {
            return apprenticeship.StartDate.Value > new DateTime(currentDateTime.UtcNow.Year, currentDateTime.UtcNow.Month, 1);
        }
    }
}