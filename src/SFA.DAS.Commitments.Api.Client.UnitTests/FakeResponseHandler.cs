using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Api.Client.UnitTests
{
    public class FakeResponseHandler : DelegatingHandler
    {
        private readonly Dictionary<TestRequest, HttpResponseMessage> _fakeResponses = new Dictionary<TestRequest, HttpResponseMessage>();

        public void AddFakeResponse(TestRequest request, HttpResponseMessage responseMessage)
        {
            _fakeResponses.Add(request, responseMessage);
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var str = "";
            if (request?.Content != null) str = await request?.Content.ReadAsStringAsync();
            var testRequest = new TestRequest(request.RequestUri, str);

            if (_fakeResponses.ContainsKey(testRequest))
            {
                return _fakeResponses[testRequest];
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound)
                           {
                               RequestMessage = request,
                               Content = new StringContent(string.Empty)
                           };
            }
        }
    }
}