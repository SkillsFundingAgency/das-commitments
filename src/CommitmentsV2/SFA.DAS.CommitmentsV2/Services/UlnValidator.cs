using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services;

public class UlnValidator(Learners.Validators.IUlnValidator validator) : IUlnValidator
{
    public UlnValidationResult Validate(string uln)
    {
        var result = validator.Validate(uln);
        return (UlnValidationResult)result;
    }
}