namespace SFA.DAS.CommitmentsV2.Data
{
    public interface IDbReadOnlyContextFactory
    {
        CommitmentsReadOnlyDbContext CreateDbContext();
    }
}