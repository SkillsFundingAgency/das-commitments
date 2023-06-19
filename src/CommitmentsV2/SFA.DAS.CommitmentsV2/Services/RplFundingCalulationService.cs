using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class RplFundingCalulationService : IRplFundingCalulationService
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly ILogger<RplFundingCalulationService> _logger;

        public RplFundingCalulationService(IDbContextFactory dbContextFactory, ILogger<RplFundingCalulationService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<RplFundingCalulation> GetRplFundingCalulations(
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
            int? fundingBandMaximum = await GetFundingBandMaximum(courseCode, startDate, frameworkFundingPeriods, standardFundingPeriods);
            decimal? percentageOfPriorLearning = CalculatePercentageOfPriorLearning(durationReducedByHours, trainingTotalHours);
            decimal? minimumPercentageReduction = CalculateMinimumPercentageOfPriorLearning(percentageOfPriorLearning);
            int? minimumPriceReduction = CalculateMinimumPriceReduction(fundingBandMaximum, minimumPercentageReduction);
            bool rplPriceReductionError = HasRplPriceReductionError(trainingTotalHours, durationReducedByHours, priceReducedBy, isDurationReducedByRpl, minimumPriceReduction);

            var rplFundingCalulation = new RplFundingCalulation
            {
                FundingBandMaximum = fundingBandMaximum,
                PercentageOfPriorLearning = percentageOfPriorLearning,
                MinimumPercentageReduction = minimumPercentageReduction,
                MinimumPriceReduction = minimumPriceReduction,
                RplPriceReductionError = rplPriceReductionError
            };

            return rplFundingCalulation;
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
            if (percentageOfPriorLearning == null)
                return null;
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
            var areSet = trainingTotalHours.HasValue && durationReducedByHours.HasValue && priceReducedBy.HasValue && isDurationReducedByRpl.HasValue;
            if (!areSet) return false;
            switch (isDurationReducedByRpl)
            {
                case true when !isDurationReducedByRpl.HasValue:
                case false when isDurationReducedByRpl.HasValue:
                    return false;
                default:
                    return true;
            }
        }
    }
}