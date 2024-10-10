namespace SFA.DAS.CommitmentsV2.Domain.Entities;

public enum UlnValidationResult
{
    Success,
    IsEmptyUlnNumber,
    IsInValidTenDigitUlnNumber,
    IsInvalidUln
}