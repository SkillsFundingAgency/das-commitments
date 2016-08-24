namespace SFA.DAS.Commitments.Application.Queries
{
    public class QueryResponse<T> : QueryResponseBase
    {
        public T Data { get; set; }
    }
}
