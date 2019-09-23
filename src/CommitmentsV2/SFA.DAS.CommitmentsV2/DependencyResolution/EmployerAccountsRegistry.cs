using SFA.DAS.EAS.Account.Api.Client;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class EmployerAccountsRegistry : DataRegistry
    {
        public EmployerAccountsRegistry()
        {
            For<IAccountApiClient>().Use(c => new AccountApiClient(c.GetInstance<AccountApiConfiguration>()));
        }
    }
}