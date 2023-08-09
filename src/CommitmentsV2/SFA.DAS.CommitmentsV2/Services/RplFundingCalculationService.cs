using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class RplFundingCalculationService : IRplFundingCalculationService
    {
        public async Task<RplFundingCalculation> GetRplFundingCalculations(
                string courseCode,
                DateTime? startDate,
                int? durationReducedByHours,
                int? trainingTotalHours,
                int? priceReducedBy,
                bool? isDurationReducedByRpl,
                DbSet<StandardFundingPeriod> standardFundingPeriods,
                DbSet<FrameworkFundingPeriod> frameworkFundingPeriods
            )
        {
            var fundingBandMaximum = await GetFundingBandMaximum(courseCode, startDate, frameworkFundingPeriods, standardFundingPeriods);
            var percentageOfPriorLearning = CalculatePercentageOfPriorLearning(durationReducedByHours, trainingTotalHours);
            var minimumPercentageReduction = CalculateMinimumPercentageOfPriorLearning(percentageOfPriorLearning);
            var minimumPriceReduction = CalculateMinimumPriceReduction(fundingBandMaximum, minimumPercentageReduction);
            var rplPriceReductionError = HasRplPriceReductionError(trainingTotalHours, durationReducedByHours, priceReducedBy, isDurationReducedByRpl, minimumPriceReduction);

            var rplFundingCalculation = new RplFundingCalculation
            {
                FundingBandMaximum = fundingBandMaximum,
                PercentageOfPriorLearning = percentageOfPriorLearning,
                MinimumPercentageReduction = minimumPercentageReduction,
                MinimumPriceReduction = minimumPriceReduction,
                RplPriceReductionError = rplPriceReductionError
            };

            return rplFundingCalculation;
        }

        private async Task<int?> GetFundingBandMaximum(string courseCode, DateTime? startDate, DbSet<FrameworkFundingPeriod> frameworkFundingPeriods, DbSet<StandardFundingPeriod> standardFundingPeriods)
        {
            if (string.IsNullOrEmpty(courseCode))
                return null;

            if (int.TryParse(courseCode, out var standardId))
            {
                var standard = await standardFundingPeriods.Where(c => c.Id.Equals(standardId)
                    && c.EffectiveFrom <= startDate && (c.EffectiveTo == null || c.EffectiveTo >= startDate))
                    .OrderByDescending(x => x.EffectiveFrom).FirstOrDefaultAsync();
                return standard?.FundingCap;
            }

            var framework = await frameworkFundingPeriods.Where(c => c.Id.Equals(courseCode)
                    && c.EffectiveFrom <= startDate && (c.EffectiveTo == null || c.EffectiveTo >= startDate))
                .OrderByDescending(x => x.EffectiveFrom).FirstOrDefaultAsync();
            return framework?.FundingCap;
        }

        private static decimal? CalculatePercentageOfPriorLearning(int? durationReducedByHours, int? trainingTotalHours)
        {
            if (durationReducedByHours == null || trainingTotalHours == null || trainingTotalHours == 0)
                return null;
            return (decimal)durationReducedByHours / trainingTotalHours * 100;
        }

        private static decimal? CalculateMinimumPercentageOfPriorLearning(decimal? percentageOfPriorLearning)
        {
            return percentageOfPriorLearning / 2;
        }

        private static bool HasRplPriceReductionError(int? trainingTotalHours, int? durationReducedByHours, int? priceReducedBy, bool? isDurationReducedByRpl, int? minimumPriceReduction)
        {
            if (!AreRplFieldsAreComplete(trainingTotalHours, durationReducedByHours, priceReducedBy, isDurationReducedByRpl))
                return false;
            return priceReducedBy < minimumPriceReduction;
        }

        private static int? CalculateMinimumPriceReduction(int? fundingBandMaximum, decimal? minimumPercentageReduction)
        {
            if (fundingBandMaximum == null || minimumPercentageReduction == null || minimumPercentageReduction == 0)
                return null;
            return (int?)(fundingBandMaximum * minimumPercentageReduction / 100);
        }

        private static bool AreRplFieldsAreComplete(int? trainingTotalHours, int? durationReducedByHours, int? priceReducedBy, bool? isDurationReducedByRpl)
        {
            return !(trainingTotalHours.HasValue && durationReducedByHours.HasValue && priceReducedBy.HasValue && isDurationReducedByRpl.HasValue);
        }
    }
}