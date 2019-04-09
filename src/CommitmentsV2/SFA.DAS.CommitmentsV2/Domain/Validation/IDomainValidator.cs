using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;

namespace SFA.DAS.CommitmentsV2.Domain.Validation
{
    public interface IDomainValidator
    {
        Task<DomainError[]> ValidateAsync<T>(T instance) where T : class;
    }
}