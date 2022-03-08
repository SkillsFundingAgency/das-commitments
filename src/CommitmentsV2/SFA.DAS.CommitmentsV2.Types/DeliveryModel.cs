using System.Text.Json.Serialization;

namespace SFA.DAS.CommitmentsV2.Types
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeliveryModel : byte
    {
        Regular = 0,
        PortableFlexiJob = 1,
    }
}