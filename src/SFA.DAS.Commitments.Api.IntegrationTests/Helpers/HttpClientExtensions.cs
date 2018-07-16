//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web.Script.Serialization;

//namespace SFA.DAS.Commitments.Api.IntegrationTests.Helpers
//{
//    public static class HttpClientExtensions
//    {
//        public static async Task<HttpResponseMessage> PostAsJsonAsync<TModel>(this HttpClient client, string requestUrl, TModel model)
//        {
//            var json = new JavaScriptSerializer().Serialize(model);
//            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
//            return await client.PostAsync(requestUrl, stringContent);
//        }
//    }
//}
