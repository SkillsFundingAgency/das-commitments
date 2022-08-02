
using System.Text.Json.Serialization;

namespace SFA.DAS.CommitmentsV2.Types
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ChangeOfPartyRequestType : byte
    {
        ChangeEmployer = 0,
        ChangeProvider = 1
    }
}
