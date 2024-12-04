using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllLearners;

public class GetAllLearnersQueryResult
{
    public List<Learner> Learners { get; }

    public int BatchNumber { get; }

    public int BatchSize { get; }

    public int TotalNumberOfBatches { get; }

    public GetAllLearnersQueryResult(List<Learner> learners, int batchNumber, int batchSize, int totalNumberOfBatches)
    {
        Learners = learners;
        BatchNumber = batchNumber;
        BatchSize = batchSize;
        TotalNumberOfBatches = totalNumberOfBatches;
    }
}