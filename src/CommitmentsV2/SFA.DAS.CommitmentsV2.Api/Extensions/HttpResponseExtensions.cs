using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SFA.DAS.CommitmentsV2.Api.Extensions
{
    public static class HttpResponseExtensions
    {
        public static Task WriteJsonAsync(this HttpResponse httpResponse, object body)
        {
            httpResponse.ContentType = "application/json";
            
            return httpResponse.WriteAsync(JsonConvert.SerializeObject(body, new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new StringEnumConverter() },
                Formatting = Formatting.Indented
            }));
        }
    }
}