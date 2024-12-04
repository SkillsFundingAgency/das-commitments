namespace SFA.DAS.CommitmentsV2.Shared.Interfaces;

public interface IMapper<in TFrom, TTo> where TFrom : class where TTo : class
{
    Task<TTo> Map(TFrom source);
}