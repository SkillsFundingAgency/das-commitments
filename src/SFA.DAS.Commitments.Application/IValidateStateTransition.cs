namespace SFA.DAS.Commitments.Application
{
    public interface IValidateStateTransition<T> where T : struct
    {
        bool IsStateTransitionValid(T initial, T target);
    }
 }
