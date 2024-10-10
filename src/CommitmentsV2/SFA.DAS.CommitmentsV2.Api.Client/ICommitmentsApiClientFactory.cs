namespace SFA.DAS.CommitmentsV2.Api.Client;

public interface ICommitmentsApiClientFactory
{
    ICommitmentsApiClient CreateClient();
}