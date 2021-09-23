using System;

namespace SFA.DAS.CommitmentsV2.Models.Api
{
    public class GetApprentice : IGetApiRequest
    {
        private readonly Guid _apprenticeId;

        public GetApprentice(Guid apprenticeId)
        {
            _apprenticeId = apprenticeId;
        }
        public string GetUrl => $"apprentices/{_apprenticeId}";
    }
}