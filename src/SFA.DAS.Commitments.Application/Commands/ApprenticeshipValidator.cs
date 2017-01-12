using System;
using System.Text.RegularExpressions;

using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands
{
    internal sealed class ApprenticeshipValidator : AbstractValidator<Api.Types.Apprenticeship>
    {
        public ApprenticeshipValidator()
        {
            var now = DateTime.Now;
            Func<string, int, bool> lengthLessThan = (str, length) => (str?.Length ?? length) < length;

            RuleFor(x => x.FirstName).NotEmpty().Must(m => lengthLessThan(m, 100));
            RuleFor(x => x.LastName).NotEmpty().Must(m => lengthLessThan(m, 100));
            RuleFor(x => x.ULN).Matches("^$|^[1-9]{1}[0-9]{9}$").Must(m => m != "9999999999");
            RuleFor(x => x.Cost).Must(CostIsValid);

            RuleFor(x => x.NINumber)
                .Matches(@"^[abceghj-prstw-z][abceghj-nprstw-z]\d{6}[abcd\s]$", RegexOptions.IgnoreCase)
                    .Unless(m => m.NINumber == null);

            RuleFor(x => x.ProviderRef).Length(0, 20);
            RuleFor(x => x.EmployerRef).Length(0, 20);

            RuleFor(r => r.EndDate)
                    .Must(BeGreaterThenStartDate)
                    .Must(m => m > now).Unless(m => m.EndDate == null);
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

        private bool BeGreaterThenStartDate(Api.Types.Apprenticeship viewModel, DateTime? date)
        {
            if (viewModel.StartDate == null || viewModel.EndDate == null) return true;

            return viewModel.StartDate < viewModel.EndDate;
        }
    }
}
