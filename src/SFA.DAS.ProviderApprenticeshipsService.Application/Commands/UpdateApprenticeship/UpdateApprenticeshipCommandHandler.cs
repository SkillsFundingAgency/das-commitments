using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Client;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Commands.UpdateApprenticeship
{
    public class UpdateApprenticeshipCommandHandler : AsyncRequestHandler<UpdateApprenticeshipCommand>
    {
        private readonly ICommitmentsApi _commitmentsApi;
        private readonly UpdateApprenticeshipCommandValidator _validator;

        public UpdateApprenticeshipCommandHandler(ICommitmentsApi commitmentsApi)
        {
            if (commitmentsApi == null)
                throw new ArgumentNullException(nameof(commitmentsApi));
            _commitmentsApi = commitmentsApi;
            _validator = new UpdateApprenticeshipCommandValidator();
        }

        protected override async Task HandleCore(UpdateApprenticeshipCommand message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new InvalidRequestException(validationResult.Errors);

            await _commitmentsApi.UpdateProviderApprenticeship(message.ProviderId, message.Apprenticeship.CommitmentId, message.Apprenticeship.Id, message.Apprenticeship);
        }
    }
}