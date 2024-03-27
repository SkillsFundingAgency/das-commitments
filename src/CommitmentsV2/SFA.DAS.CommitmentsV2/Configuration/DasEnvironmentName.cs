namespace SFA.DAS.CommitmentsV2.Configuration;

public static class DasEnvironmentName
{
    public static readonly string AcceptanceTest = nameof(AcceptanceTest);
    public static readonly string Test = nameof(Test);
    public static readonly string Test2 = nameof(Test2);
    public static readonly string PreProduction = nameof(PreProduction);
    public static readonly string ModelOffice = nameof(ModelOffice);
    public static readonly string Demonstration = nameof(Demonstration);

    public static readonly Dictionary<string, string> Map = new(StringComparer.CurrentCultureIgnoreCase)
    {
        { "LOCAL", "Development" },
        { "AT", AcceptanceTest },
        { "TEST", Test },
        { "TEST2", Test2 },
        { "PREPROD", PreProduction },
        { "PROD", "Production" },
        { "MO", ModelOffice },
        { "DEMO", Demonstration }
    };
}