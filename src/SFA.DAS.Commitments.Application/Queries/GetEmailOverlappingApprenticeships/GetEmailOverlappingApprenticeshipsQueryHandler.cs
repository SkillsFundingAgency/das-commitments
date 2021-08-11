using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Queries.GetEmailOverlappingApprenticeships
{
    public sealed class GetEmailOverlappingApprenticeshipsQueryHandler : IAsyncRequestHandler<GetEmailOverlappingApprenticeshipsRequest, GetEmailOverlappingApprenticeshipsResponse>
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ICommitmentsLogger _logger;
        private readonly AbstractValidator<GetEmailOverlappingApprenticeshipsRequest> _validator;

        public GetEmailOverlappingApprenticeshipsQueryHandler(IApprenticeshipRepository apprenticeshipRepository,
            AbstractValidator<GetEmailOverlappingApprenticeshipsRequest> validator,
            ICommitmentsLogger logger)
        {
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
            _logger = logger;
        }

        public async Task<GetEmailOverlappingApprenticeshipsResponse> Handle(GetEmailOverlappingApprenticeshipsRequest request)
        {
            _logger.Info("Performing overlap validation for bulk upload");

            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var result = new GetEmailOverlappingApprenticeshipsResponse
            {
                Data = new List<OverlappingEmail>()
            };

            var emails = request.OverlappingEmailApprenticeshipRequests.Where(x => !string.IsNullOrWhiteSpace(x.Email)).Select(x => x.Email).ToList();

            if (!emails.Any())
                return result;

            var emailToValidate = new List<EmailToValidate>();
            var i = 0;
            foreach (var apprenticeship in request.OverlappingEmailApprenticeshipRequests)
            {
                emailToValidate.Add(new EmailToValidate(apprenticeship.Email, apprenticeship.StartDate, apprenticeship.EndDate, i, i));
                i++;
            }

            var apprenticeshipEmailOverlapResponse = await _apprenticeshipRepository.GetEmailOverlaps(emailToValidate);

            foreach (var apprenticeshipEmailOverlap in apprenticeshipEmailOverlapResponse)
            {
                if (apprenticeshipEmailOverlap.OverlapStatus != OverlapStatus.None)
                {
                    _logger.Info($"Validation failed for: {apprenticeshipEmailOverlap.StartDate:MMM yyyy} - {apprenticeshipEmailOverlap.EndDate:MMM yyyy} Reason: {apprenticeshipEmailOverlap.OverlapStatus} " +
                                    $"with Apprenticeship Id: {apprenticeshipEmailOverlap.Id} {apprenticeshipEmailOverlap.StartDate:MMM yyyy} - {apprenticeshipEmailOverlap.EndDate:MMM yyyy}");

                    result.Data.Add(new OverlappingEmail
                    {
                        Id = apprenticeshipEmailOverlap.RowId,
                        RowId = apprenticeshipEmailOverlap.RowId,
                        FirstName = apprenticeshipEmailOverlap.FirstName,
                        LastName = apprenticeshipEmailOverlap.LastName,
                        Email = apprenticeshipEmailOverlap.Email,
                        StartDate = apprenticeshipEmailOverlap.StartDate,
                        EndDate = apprenticeshipEmailOverlap.EndDate,
                        DateOfBirth = apprenticeshipEmailOverlap.DateOfBirth,
                        CohortId = apprenticeshipEmailOverlap.CohortId,
                    });
                }
            }

            return result;
        }
    }
}