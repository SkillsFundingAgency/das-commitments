using System.Text.Json.Serialization;

namespace SFA.DAS.CommitmentsV2.Types
{
    [JsonConverter(typeof(JsonStringEnumConverter))]

    public enum DeliveryModel : byte
    {
        Normal = 0,
        Flexible = 1
    }
}