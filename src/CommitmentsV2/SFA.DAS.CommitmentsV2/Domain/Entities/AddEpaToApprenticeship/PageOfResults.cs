namespace SFA.DAS.CommitmentsV2.Domain.Entities.AddEpaToApprenticeship
{
    public class PageOfResults<T>
    {
        public int PageNumber { get; set; }

        public int TotalNumberOfPages { get; set; }

        public T[] Items { get; set; }
    }
}
