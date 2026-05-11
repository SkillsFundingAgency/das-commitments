using System.Buffers;

namespace SFA.DAS.CommitmentsV2.Validation;

internal static class ApprovedUriValidation
{
    public const int MaxLength = 2048;

    private static readonly SearchValues<char> AllowedChars = SearchValues.Create(
        "/-ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");

    public static bool IsValidOptional(string value)
    {
        if (string.IsNullOrEmpty(value))
            return true;

        return value.Length <= MaxLength
               && value.AsSpan().IndexOfAnyExcept(AllowedChars) < 0;
    }
}
