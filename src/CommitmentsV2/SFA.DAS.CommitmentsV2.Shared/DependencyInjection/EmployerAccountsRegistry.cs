using SFA.DAS.EAS.Account.Api.Client;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.Shared.DependencyInjection
{
    public class EmployerAccountsRegistry : Registry
    {
        public EmployerAccountsRegistry()
        {
            For<IAccountApiClient>().Use(c => new AccountApiClient(c.GetInstance<AccountApiConfiguration>()));
        }
    }
}