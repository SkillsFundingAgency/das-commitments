using System.Net;

namespace SFA.DAS.Commitments.Api.Core
{
    public class OrchestratorResponse
    {
        public OrchestratorResponse()
        {
            Status = HttpStatusCode.OK;
        }
        public HttpStatusCode Status { get; set; }

    }

    public class OrchestratorResponse<T> : OrchestratorResponse
    {
        public T Data { get; set; }
    }
}