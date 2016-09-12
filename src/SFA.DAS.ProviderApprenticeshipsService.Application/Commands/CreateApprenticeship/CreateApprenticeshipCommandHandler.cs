using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Client;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Commands.CreateApprenticeship
{
    public class CreateApprenticeshipCommandHandler : AsyncRequestHandler<CreateApprenticeshipCommand>
    {
        private readonly ICommitmentsApi _commitmentsApi;
        private readonly CreateApprenticeshipCommandValidator _validator;

        public CreateApprenticeshipCommandHandler(ICommitmentsApi commitmentsApi)
        {
            if (commitmentsApi == null)
                throw new ArgumentNullException(nameof(commitmentsApi));
            _commitmentsApi = commitmentsApi;
            _validator = new CreateApprenticeshipCommandValidator();
        }

        protected override async Task HandleCore(CreateApprenticeshipCommand message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new InvalidRequestException(validationResult.Errors);

            await _commitmentsApi.CreateProviderApprenticeship(message.ProviderId, message.Apprenticeship.CommitmentId, message.Apprenticeship);
        }
    }
}