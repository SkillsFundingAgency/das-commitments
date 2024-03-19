using System.Text.Json.Serialization;

namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi
{
    public interface IPostApiRequest : IPostApiRequest<object>
    {
    }

    public interface IPostApiRequest<TData>
    {
        [JsonIgnore]
        string PostUrl { get; }
        TData Data { get; set; }
    }
}
