using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetStandardOptions
{
    public class GetStandardOptionsQuery : IRequest<GetStandardOptionsResult>
    {
        public string StandardUId { get; set; }

        public GetStandardOptionsQuery(string standardUId)
        {
            StandardUId = standardUId;
        }
    }
}
