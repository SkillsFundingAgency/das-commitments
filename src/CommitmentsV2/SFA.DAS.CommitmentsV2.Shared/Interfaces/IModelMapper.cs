namespace SFA.DAS.CommitmentsV2.Shared.Interfaces;

public interface IModelMapper
{
    Task<T> Map<T>(object source) where T : class;
}