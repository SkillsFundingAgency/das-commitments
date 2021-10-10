using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class UlnValidator : IUlnValidator
    {
        private readonly Learners.Validators.IUlnValidator _validator;

        public UlnValidator(Learners.Validators.IUlnValidator validator)
        {
            _validator = validator;
        }
        public UlnValidationResult Validate(string uln)
        {
            var result = _validator.Validate(uln);
            return (UlnValidationResult)result;
        }
    }
} 