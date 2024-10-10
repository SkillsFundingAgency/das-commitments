namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;

public class PageOfResults<T>
{
    public int PageNumber { get; set; }
    public int TotalNumberOfPages { get; set; }
    public T[] Items { get; set; }
}