using System.Buffers;

namespace SFA.DAS.CommitmentsV2.Validation;

internal static class ApprovedUriValidation
{
    public const int MaxLength = 2048;

    private static readonly SearchValues<char> InvalidChars =
        SearchValues.Create(['<', '>', '"', '\'', '`', '\\']);

    public static bool IsValidOptional(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        var trimmed = value.Trim();

        return trimmed.Length <= MaxLength
               && !trimmed.AsSpan().ContainsAny(InvalidChars)
               && !trimmed.Any(char.IsControl)
               && Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
               && uri.Scheme is "http" or "https";
    }
}
