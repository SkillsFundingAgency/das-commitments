using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;

namespace SFA.DAS.CommitmentsV2.Shared.Services
{
    /// <summary>
    /// Commitments stub of EAS Account Api Client
    /// https://github.com/SkillsFundingAgency/das-employerapprenticeshipsservice/blob/f506d4e401109312d4d787da8a9b004060326912/src/SFA.DAS.Account.Api.Client/AccountApiClient.cs
    /// </summary>
    public class StubAccountApiClient : IAccountApiClient
    {
        private readonly HttpClient _httpClient;

        public StubAccountApiClient()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:3999/accounts-api/api/") };
        }

        public Task<AccountDetailViewModel> GetAccount(string hashedAccountId)
        {
            throw new NotImplementedException();
        }

        public async Task<AccountDetailViewModel> GetAccount(long accountId)
        {
            var url = $"accounts/internal/{accountId}";
            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AccountDetailViewModel>(json);
        }

        public Task<ICollection<TeamMemberViewModel>> GetAccountUsers(string accountId)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<TeamMemberViewModel>> GetAccountUsers(long accountId)
        {
            var url = $"accounts/internal/{accountId}/users";
            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ICollection<TeamMemberViewModel>>(json);
        }

        public async Task<EmployerAgreementView> GetEmployerAgreement(string accountId, string legalEntityId, string agreementId)
        {
            var url = $"accounts/{accountId}/legalEntities/{legalEntityId}/agreements/{agreementId}/agreement";
            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<EmployerAgreementView>(json);
        }

        public async Task<ICollection<ResourceViewModel>> GetLegalEntitiesConnectedToAccount(string accountId)
        {
           var url = $"accounts/{accountId}/legalentities";
            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ResourceViewModel>>(json);
        }

        public async Task<LegalEntityViewModel> GetLegalEntity(string accountId, long id)
        {
            var url = $"accounts/{accountId}/legalentities/{id}";
            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<LegalEntityViewModel>(json);
        }

        public async Task<ICollection<LegalEntityViewModel>> GetLegalEntityDetailsConnectedToAccount(string accountId)
        {
           var url = $"accounts/{accountId}/legalentities";
            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<LegalEntityViewModel>>(json);
        }

        public Task<ICollection<LevyDeclarationViewModel>> GetLevyDeclarations(string accountId)
        {
            throw new NotImplementedException();
        }

        public Task<PagedApiResponseViewModel<AccountLegalEntityViewModel>> GetPageOfAccountLegalEntities(int pageNumber = 1, int pageSize = 1000)
        {
            throw new NotImplementedException();
        }

        public Task<PagedApiResponseViewModel<AccountWithBalanceViewModel>> GetPageOfAccounts(int pageNumber = 1, int pageSize = 1000, DateTime? toDate = null)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<ResourceViewModel>> GetPayeSchemesConnectedToAccount(string accountId)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetResource<T>(string uri)
        {
            throw new NotImplementedException();
        }

        public Task<StatisticsViewModel> GetStatistics()
        {
            throw new NotImplementedException();
        }

        public Task<TransactionsViewModel> GetTransactions(string accountId, int year, int month)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<TransactionSummaryViewModel>> GetTransactionSummary(string accountId)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<TransferConnectionViewModel>> GetTransferConnections(string accountHashedId)
        {
            var url = $"accounts/{accountHashedId}/transfers/connections";
            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ICollection<TransferConnectionViewModel>>(json);
        }

        public Task<ICollection<AccountDetailViewModel>> GetUserAccounts(string userId)
        {
            throw new NotImplementedException();
        }

        public Task Ping()
        {
            throw new NotImplementedException();
        }

    }
}
