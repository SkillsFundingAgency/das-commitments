using SFA.DAS.Commitments.Domain.Api.Requests;

namespace SFA.DAS.Commitments.Infrastructure.Api.Requests
{
    public class GetEpaoOrganisationsRequest: IGetApiRequest
    {
        public string GetUrl => "epaos";
    }
    
}