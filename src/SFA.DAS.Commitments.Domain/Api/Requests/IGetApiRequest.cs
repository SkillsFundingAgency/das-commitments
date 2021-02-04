using Newtonsoft.Json;

namespace SFA.DAS.Commitments.Domain.Api.Requests
{
    public interface IGetApiRequest
    {
        [JsonIgnore]
        string GetUrl { get;}
    }
}