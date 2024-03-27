namespace SFA.DAS.Commitments.Support.SubSite.Extensions
{
    public static class DecimalFormatExtensions
    {
        public static string ToGdsCurrencyFormat(this Decimal? value)
        {
            return value.HasValue ? string.Format(new System.Globalization.CultureInfo("en-GB", false), "{0:c0}", value) : string.Empty;
        }

    }
}