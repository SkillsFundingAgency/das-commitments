using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Models;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IRplFundingCalculationService
    {
        Task<RplFundingCalculation> GetRplFundingCalculations(string courseCode, DateTime? startDate, int? durationReducedByHours, int? trainingTotalHours, int? priceReducedBy, bool? isDurationReducedByRpl, DbSet<StandardFundingPeriod> standardFundingPeriods, DbSet<FrameworkFundingPeriod> frameworkFundingPeriods);
    }
}