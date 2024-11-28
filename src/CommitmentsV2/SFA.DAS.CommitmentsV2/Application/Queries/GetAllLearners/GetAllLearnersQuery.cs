namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllLearners;

public class GetAllLearnersQuery : IRequest<GetAllLearnersQueryResult>
{
    public DateTime? SinceTime { get; set; }
    public int BatchNumber { get; set; }
    public int BatchSize { get; set; }

    public GetAllLearnersQuery(DateTime? sinceTime, int batchNumber, int batchSize)
    {
        SinceTime = sinceTime;
        BatchNumber = batchNumber;
        BatchSize = batchSize;
    }
}