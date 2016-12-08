namespace SFA.DAS.Commitments.Domain.Interfaces
{
    public interface IRequestContext
    {
        string Url { get; }
        string IpAddress { get; }
    }
}
