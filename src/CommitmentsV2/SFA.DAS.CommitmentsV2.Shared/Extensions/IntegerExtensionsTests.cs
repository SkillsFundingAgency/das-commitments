namespace SFA.DAS.CommitmentsV2.Shared.Extensions;

public static class IntegerExtensions
{
    public static string ToGdsCostFormat(this int value)
    {
        return $"£{value:n0}";
    }
}