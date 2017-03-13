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
                foreach (var apprenticeship in apprenticeships.Where(x=> x.Uln.Equals(request.Uln, StringComparison.InvariantCultureIgnoreCase)))
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
                    else if (IsApprenticeshipDateBefore(request.DateFrom, apprenticeship.StartDate) &&
                             IsApprenticeshipDateAfter(request.DateTo, apprenticeship.EndDate))
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

    }
}
