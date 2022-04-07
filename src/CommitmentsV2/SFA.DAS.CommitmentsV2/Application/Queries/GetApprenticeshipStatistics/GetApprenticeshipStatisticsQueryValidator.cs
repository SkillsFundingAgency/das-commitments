using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatistics
{
    public class GetApprenticeshipStatisticsQueryValidator : AbstractValidator<GetApprenticeshipStatisticsQuery>
    {
        public GetApprenticeshipStatisticsQueryValidator()
        {
            RuleFor(x => x.LastNumberOfDays).GreaterThan(0);
        }
    }
}
