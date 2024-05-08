namespace SFA.DAS.CommitmentsV2.Mapping
{
    public interface IOldMapper<in TFrom, TTo> where TFrom: class where TTo: class
    {
        Task<TTo> Map(TFrom source);
    }
}
