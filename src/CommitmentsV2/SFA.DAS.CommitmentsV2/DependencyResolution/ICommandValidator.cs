using FluentValidation;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public interface ICommandValidator<T> : IValidator<T>
    {

    }
}