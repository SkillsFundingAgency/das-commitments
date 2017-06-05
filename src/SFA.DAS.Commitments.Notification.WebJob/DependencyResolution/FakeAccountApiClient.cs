using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;

namespace SFA.DAS.Commitments.Notification.WebJob.DependencyResolution
{
    public class FakeAccountApiClient : IAccountApiClient
    {

        public Task<T> GetResource<T>(string uri) where T : SFA.DAS.EAS.Account.Api.Types.IAccountResource
        {
            throw new NotImplementedException();
        }

        Task<PagedApiResponseViewModel<AccountWithBalanceViewModel>> IAccountApiClient.GetPageOfAccounts(int pageNumber, int pageSize, DateTime? toDate)
        {
            throw new NotImplementedException();
        }

        async Task<ICollection<TeamMemberViewModel>> IAccountApiClient.GetAccountUsers(string accountId)
        {
            return await Task.FromResult(new List<TeamMemberViewModel>
                {
                    new TeamMemberViewModel
                        {
                            Name = "",
                            Email = "",
                            Role = "WhatEver"
                        }
                });
        }

        Task<ICollection<AccountDetailViewModel>> IAccountApiClient.GetUserAccounts(string userId)
        {
            throw new NotImplementedException();
        }
    }
}