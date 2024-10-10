using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface IUlnValidator
{
    UlnValidationResult Validate(string uln);
}