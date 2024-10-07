using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Domain.Exceptions;

public static class DomainErrorExtensions
{
    public static void ThrowIfAny(this List<DomainError> errors)
    {
        if (errors.Count != 0)
        {
            throw new DomainException(errors);
        }
    }

    public static void ThrowIfAny(this List<BulkUploadValidationError> errors)
    {
        if (errors.Count != 0)
        {
            throw new BulkUploadDomainException(errors);
        }
    }
}