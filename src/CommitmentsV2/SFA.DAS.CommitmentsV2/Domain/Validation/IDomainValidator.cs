using SFA.DAS.CommitmentsV2.Domain.Exceptions;

namespace SFA.DAS.CommitmentsV2.Domain.Validation
{
    public interface IDomainValidator
    {
        DomainError[] Validate<T>(T instance) where T : class;
    }
}