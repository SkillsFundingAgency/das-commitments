using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;

namespace SFA.DAS.Commitments.Infrastructure.UnitTests.Services.EmployerAccountsService
{
    [TestFixture]
    public class WhenIGetAccount
    {
        private Mock<IAccountApiClient> _accountApiClient;
        private Infrastructure.Services.EmployerAccountsService _employerAccountsService;

        [SetUp]
        public void SetUp()
        {
            _accountApiClient = new Mock<IAccountApiClient>();
            _employerAccountsService = new Infrastructure.Services.EmployerAccountsService(_accountApiClient.Object);
        }
        
        [Test]
        public async Task ThenAccountIsReturned()
        {
            const int accountId = 1;
            const ApprenticeshipEmployerType apprenticeshipEmployerType = ApprenticeshipEmployerType.Levy;

            _accountApiClient.Setup(c => c.GetAccount(accountId)).ReturnsAsync(new AccountDetailViewModel
            {
                AccountId = accountId,
                ApprenticeshipEmployerType = apprenticeshipEmployerType.ToString()
            });
            
            var account = await _employerAccountsService.GetAccount(accountId);
            
            Assert.AreEqual(accountId, account.Id);
            Assert.AreEqual(apprenticeshipEmployerType, account.ApprenticeshipEmployerType);
        }
    }
}