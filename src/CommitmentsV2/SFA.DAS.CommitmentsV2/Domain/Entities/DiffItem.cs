namespace SFA.DAS.CommitmentsV2.Domain.Entities;

public class DiffItem
{
    public string PropertyName { get; set; }
    public object InitialValue { get; set; }
    public object UpdatedValue { get; set; }
}