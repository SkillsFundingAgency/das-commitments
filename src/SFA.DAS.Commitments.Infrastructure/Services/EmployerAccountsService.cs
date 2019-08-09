using System.Threading.Tasks;
using SFA.DAS.Commitments.Application.Extensions;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.EAS.Account.Api.Client;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class EmployerAccountsService : IEmployerAccountsService
    {
        private readonly IAccountApiClient _accountApiClient;

        public EmployerAccountsService(IAccountApiClient accountApiClient)
        {
            _accountApiClient = accountApiClient;
        }

        public async Task<Account> GetAccount(long accountId)
        {
            var account = await _accountApiClient.GetAccount(accountId);
            
            var response = new Account
            {
                Id = account.AccountId,
                ApprenticeshipEmployerType = account.ApprenticeshipEmployerType.ToEnum<ApprenticeshipEmployerType>()
            };

            return response;
        }
    }
}