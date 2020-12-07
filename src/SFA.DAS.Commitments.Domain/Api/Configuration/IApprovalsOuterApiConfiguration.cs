namespace SFA.DAS.Commitments.Domain.Api.Configuration
{
    public interface IApprovalsOuterApiConfiguration
    {
        string PingUrl { get; set; }
        string Key { get; set; }
        string BaseUrl { get; set; }
    }
}