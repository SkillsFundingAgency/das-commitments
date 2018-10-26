using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.ApprovedApprenticeship;

namespace SFA.DAS.Commitments.Application.Queries.GetApprovedApprenticeship
{
    public class GetApprovedApprenticeshipQueryHandler : IAsyncRequestHandler<GetApprovedApprenticeshipRequest, GetApprovedApprenticeshipResponse>
    {
        private readonly IApprovedApprenticeshipRepository _approvedApprenticeshipRepository;
        private readonly AbstractValidator<GetApprovedApprenticeshipRequest> _validator;

        public GetApprovedApprenticeshipQueryHandler(IApprovedApprenticeshipRepository approvedApprenticeshipRepository,
            AbstractValidator<GetApprovedApprenticeshipRequest> validator)
        {
            _validator = validator;
            _approvedApprenticeshipRepository = approvedApprenticeshipRepository;
        }

        public async Task<GetApprovedApprenticeshipResponse> Handle(GetApprovedApprenticeshipRequest request)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var result = await _approvedApprenticeshipRepository.Get(request.ApprenticeshipId);

            CheckAuthorisation(request.Caller, result);

            return new GetApprovedApprenticeshipResponse
            {
                Data = result
            };
        }

        private void CheckAuthorisation(Caller caller, ApprovedApprenticeship apprenticeship)
        {
            switch (caller.CallerType)
            {
                case CallerType.Provider:
                    if (apprenticeship.ProviderId != caller.Id)
                        throw new UnauthorizedException($"Provider {caller.Id} not authorised to access apprenticeship {apprenticeship.Id}, expected provider {apprenticeship.ProviderId}");
                    break;
                case CallerType.Employer:
                    if (apprenticeship.EmployerAccountId != caller.Id)
                        throw new UnauthorizedException($"Employer {caller.Id} not authorised to access apprenticeship {apprenticeship.Id}, expected employer {apprenticeship.EmployerAccountId}");
                    break;
                default:
                    throw new UnauthorizedException($"Caller {caller.CallerType} {caller.Id} not authorised to access apprenticeship {apprenticeship.Id}");
            }
        }
    }
}
