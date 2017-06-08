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

        async Task<ICollection<TeamMemberViewModel>> IAccountApiClient.GetAccountUsers(long accountId)
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

        public Task<AccountDetailViewModel> GetAccount(string hashedAccountId)
        {
            throw new NotImplementedException();
        }

        public Task<AccountDetailViewModel> GetAccount(long accountId)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<TeamMemberViewModel>> GetAccountUsers(string accountId)
        {
            throw new NotImplementedException();
        }

        public Task<LegalEntityViewModel> GetLegalEntity(string accountId, long id)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<ResourceViewModel>> GetLegalEntitiesConnectedToAccount(string accountId)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<ResourceViewModel>> GetPayeSchemesConnectedToAccount(string accountId)
        {
            throw new NotImplementedException();
        }

        public Task<EmployerAgreementView> GetEmployerAgreement(string accountId, string legalEntityId, string agreementId)
        {
            throw new NotImplementedException();
        }
    }
}