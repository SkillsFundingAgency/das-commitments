using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IRplFundingCalculationService
    {
        Task<RplFundingCalculation> GetRplFundingCalculations(string courseCode, DateTime? startDate, int? durationReducedByHours, int? trainingTotalHours, int? priceReducedBy, bool? isDurationReducedByRpl, DbSet<StandardFundingPeriod> standardFundingPeriods, DbSet<FrameworkFundingPeriod> frameworkFundingPeriods);
    }
}