using System;
using System.Text.Json.Serialization;

namespace SFA.DAS.CommitmentsV2.Types
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeliveryModel : byte
    {
        Regular = 0,
        PortableFlexiJob = 1,
        
        [Obsolete("Use `Regular` instead of `Normal`", true)] 
        Normal = Regular,
        
        [Obsolete("Use `PortableFlexiJob` instead of `Flexible`", true)] 
        Flexible = PortableFlexiJob,
    }
}