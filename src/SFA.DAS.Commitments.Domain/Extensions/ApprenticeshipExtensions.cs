using SFA.DAS.Commitments.Domain.Interfaces;
using System;

namespace SFA.DAS.Commitments.Domain.Entities
{
    public static class ApprenticeshipExtensions
    {
        public static bool IsWaitingToStart(this Apprenticeship apprenticeship, ICurrentDateTime currentDateTime)
        {
            return apprenticeship.StartDate.Value > new DateTime(currentDateTime.Now.Year, currentDateTime.Now.Month, 1);
        }
    }
}
