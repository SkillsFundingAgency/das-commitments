using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi
{
    public class GetProviderUsersRequest : IGetApiRequest
    {
        public long Ukprn { get; }

        public GetProviderUsersRequest(long ukprn)
        {
            Ukprn = ukprn;
        }
        public string GetUrl => $"providers/{Ukprn}/users";
    }
}
