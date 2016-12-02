using System.Linq;
using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands
{
    internal sealed class ApprenticeshipValidator : AbstractValidator<Api.Types.Apprenticeship>
    {
        public ApprenticeshipValidator()
        {
            RuleFor(x => x.ULN).Must(ULNIsValid);
            RuleFor(x => x.Cost).Must(CostIsValid);
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
        }

        private bool CostIsValid(decimal? cost)
        {
            if (cost.HasValue)
            {
                if (cost <= 0 || HasGreaterThan2DecimalPlaces(cost.Value))
                    return false;
            }

            return true;
        }

        private static bool HasGreaterThan2DecimalPlaces(decimal cost)
        {
            return decimal.GetBits(cost)[3] >> 16 > 2;
        }

        private bool ULNIsValid(string uln)
        {
            if (uln != null)
            {
                if (uln.Length != 10 || !uln.All(x => char.IsDigit(x)))
                    return false;
            }

            return true;
        }
    }
}
