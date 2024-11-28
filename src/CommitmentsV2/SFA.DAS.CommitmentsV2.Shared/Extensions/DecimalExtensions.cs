namespace SFA.DAS.CommitmentsV2.Shared.Extensions;

public static class DecimalExtensions
{
    public static string ToGdsCostFormat(this decimal value)
    {
        return $"£{value:n0}";
    }
}