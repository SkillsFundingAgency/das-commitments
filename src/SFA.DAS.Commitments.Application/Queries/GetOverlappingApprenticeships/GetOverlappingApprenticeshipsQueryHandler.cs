using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships
{
    public sealed class GetOverlappingApprenticeshipsQueryHandler : IAsyncRequestHandler<GetOverlappingApprenticeshipsRequest, GetOverlappingApprenticeshipsResponse>
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly AbstractValidator<GetOverlappingApprenticeshipsRequest> _validator;

        public GetOverlappingApprenticeshipsQueryHandler(IApprenticeshipRepository apprenticeshipRepository, AbstractValidator<GetOverlappingApprenticeshipsRequest> validator)
        {
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
        }

        public async Task<GetOverlappingApprenticeshipsResponse> Handle(GetOverlappingApprenticeshipsRequest query)
        {
            var validationResult = _validator.Validate(query);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var result = new GetOverlappingApprenticeshipsResponse
            {
                Data = new List<OverlappingApprentice>()
            };

            var ulns = query.OverlappingApprenticeshipRequests.Select(x => x.Uln);

            var apprenticeships = await _apprenticeshipRepository.GetActiveApprenticeshipsByUlns(ulns);

            foreach (var request in query.OverlappingApprenticeshipRequests)
            {
                foreach (var apprenticeship in apprenticeships.Where(x=> x.Uln.Equals(request.Uln, StringComparison.InvariantCultureIgnoreCase) && x.Id != request.ExcludeApprenticeshipId))
                {
                    var overlapsStart = IsApprenticeshipDateBetween(request.DateFrom, apprenticeship.StartDate, apprenticeship.EndDate);
                    var overlapsEnd = IsApprenticeshipDateBetween(request.DateTo, apprenticeship.StartDate, apprenticeship.EndDate);

                    //Contained
                    if (overlapsStart && overlapsEnd)
                    {
                        result.Data.Add(new OverlappingApprentice
                        {
                            //todo: flesh out
                        });
                    }
                    //Overlap start date
                    else if (overlapsStart)
                    {
                        result.Data.Add(new OverlappingApprentice
                        {
                            //todo: flesh out
                        });
                    }
                    //Overlap end date
                    else if (overlapsEnd)
                    {
                        result.Data.Add(new OverlappingApprentice
                        {
                            //todo: flesh out
                        });
                    }
                    //Straddle
                    else if (IsApprenticeshipDateStraddle(request.DateFrom, request.DateTo, apprenticeship.StartDate, apprenticeship.EndDate))
                    {
                        result.Data.Add(new OverlappingApprentice
                        {
                            //todo: flesh out
                        });
                    }
                }
            }

            return result;
        }

        public static bool IsApprenticeshipDateBetween(DateTime dateToCheck, DateTime dateFrom, DateTime dateTo)
        {
            return (IsApprenticeshipDateAfter(dateToCheck, dateFrom) && IsApprenticeshipDateBefore(dateToCheck, dateTo));
        }

        private static bool IsApprenticeshipDateBefore(DateTime dateToCheck, DateTime checkAgainstDate)
        {
            if (dateToCheck.Year < checkAgainstDate.Year) return true;

            if (dateToCheck.Year > checkAgainstDate.Year) return false;

            if (dateToCheck.Month < checkAgainstDate.Month) return true;

            if (dateToCheck.Month > checkAgainstDate.Month) return false;

            return false;
        }

        private static bool IsApprenticeshipDateAfter(DateTime dateToCheck, DateTime checkAgainstDate)
        {
            if (dateToCheck.Year > checkAgainstDate.Year) return true;

            if (dateToCheck.Year < checkAgainstDate.Year) return false;

            if (dateToCheck.Month > checkAgainstDate.Month) return true;

            if (dateToCheck.Month < checkAgainstDate.Month) return false;

            return false;
        }

        private static bool IsApprenticeshipDateStraddle(DateTime date1Start, DateTime date1End, DateTime date2Start,
            DateTime date2End)
        {
            //does date 1 straddle date 2?
            if (IsApprenticeshipDateBefore(date1Start, date2Start) &&
                IsApprenticeshipDateAfter(date1End, date2End))
            {
                return true;
            }

            //In case of same month and year, if the dates span at least 1 whole month then they must straddle
            if (IsSameMonthYear(date1Start, date2Start) || IsSameMonthYear(date1End, date2End))
            {
                //if >= 1 month shared
                var startMonth = (date1Start.Year * 12) + date1Start.Month;
                var endMonth = (date1End.Year * 12) + date1End.Month;

                if ((endMonth - startMonth) >= 2)
                {
                    return true;
                }
            }
            
            return false;
        }

        private static bool IsSameMonthYear(DateTime date1, DateTime date2)
        {
            return (date1.Month == date2.Month && date1.Year == date2.Year);
        }

    }
}
