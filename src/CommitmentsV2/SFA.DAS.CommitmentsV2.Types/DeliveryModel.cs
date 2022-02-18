using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SFA.DAS.CommitmentsV2.Types
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeliveryModel : byte
    {
        Normal = 0,
        Flexible = 1
    }
}
