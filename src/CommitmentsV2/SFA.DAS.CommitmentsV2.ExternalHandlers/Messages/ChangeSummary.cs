using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.Messages;

public class ChangeSummary
{
    public List<FieldChange> Changes { get; init; } = [];
    public bool HasChanges => Changes.Count > 0;
}

public class FieldChange
{
    public string FieldName { get; init; } = string.Empty;
    public object? OldValue { get; init; } = string.Empty;
    public object? NewValue { get; init; } = string.Empty;
} 