namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IStateService
    {
        Dictionary<string, object> GetState(object item);
    }
}
