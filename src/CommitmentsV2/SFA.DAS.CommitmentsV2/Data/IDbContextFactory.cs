namespace SFA.DAS.CommitmentsV2.Data
{
    public interface IDbContextFactory
    {
        ProviderCommitmentsDbContext CreateAccountsDbContext();
    }
}
