using Microsoft.Extensions.Configuration;
using SFA.DAS.CommitmentsV2.Shared.Services;
using SFA.DAS.EAS.Account.Api.Client;
using StructureMap;
using StructureMap.Building.Interception;

namespace SFA.DAS.CommitmentsV2.Shared.DependencyInjection
{
    //public class EmployerAccountsRegistry : Registry
    //{
    //    public EmployerAccountsRegistry()
    //    {
    //        For<IAccountApiClient>().Use(c => new AccountApiClient(c.GetInstance<AccountApiConfiguration>()));

    //        For<StubAccountApiClient>().Use<StubAccountApiClient>().Singleton();
    //        Toggle<IAccountApiClient, StubAccountApiClient>("UseStubAccountApiClient");
    //    }

    //    private void Toggle<TPluginType, TConcreteType>(string configurationKey) where TConcreteType : TPluginType
    //    {
    //        For<TPluginType>().InterceptWith(new FuncInterceptor<TPluginType>((c, o) => c.GetInstance<IConfiguration>().GetValue<bool>(configurationKey) ? c.GetInstance<TConcreteType>() : o));
    //    }
    //}
}