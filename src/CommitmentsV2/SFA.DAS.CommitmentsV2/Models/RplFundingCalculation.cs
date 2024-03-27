namespace SFA.DAS.CommitmentsV2.Models
{
    public class RplFundingCalculation
    {
        public int? FundingBandMaximum { get; set; }
        public decimal? PercentageOfPriorLearning { get; set; }
        public decimal? MinimumPercentageReduction { get; set; }
        public int? MinimumPriceReduction { get; set; }
        public bool RplPriceReductionError { get; set; }
    }
}
