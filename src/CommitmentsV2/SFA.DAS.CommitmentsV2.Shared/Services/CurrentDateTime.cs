using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Shared.Services;

public sealed class CurrentDateTime : ICurrentDateTime
{
    private readonly DateTime? _time;

    public DateTime UtcNow => _time ?? DateTime.UtcNow;

    public CurrentDateTime()
    {
    }

    public CurrentDateTime(DateTime? time)
    {
        _time = time;
    }
}