namespace SFA.DAS.CommitmentsV2.Data.LinqPad;

public class ProviderCommitmentsDesignTimeDbContext : ProviderCommitmentsDbContext
{
    public ProviderCommitmentsDesignTimeDbContext(string connectionString)
        : base(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseSqlServer(connectionString)                
                .AddInterceptors(new AzureAdTokenInterceptor())
                .Options)
    {
    }
}

