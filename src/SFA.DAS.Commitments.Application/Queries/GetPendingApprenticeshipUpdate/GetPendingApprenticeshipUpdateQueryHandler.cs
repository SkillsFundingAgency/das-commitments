using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Queries.GetPendingApprenticeshipUpdate
{
    public class GetPendingApprenticeshipUpdateQueryHandler: IAsyncRequestHandler<GetPendingApprenticeshipUpdateRequest,GetPendingApprenticeshipUpdateResponse>
    {
        private readonly AbstractValidator<GetPendingApprenticeshipUpdateRequest> _validator;
        private readonly IApprenticeshipUpdateRepository _apprenticeshipUpdateRepository;

        public GetPendingApprenticeshipUpdateQueryHandler(AbstractValidator<GetPendingApprenticeshipUpdateRequest> validator, IApprenticeshipUpdateRepository apprenticeshipUpdateRepository)
        {
            if (validator == null)
                throw new ArgumentNullException();
            if(apprenticeshipUpdateRepository==null)
                throw new ArgumentNullException();

            _validator = validator;
            _apprenticeshipUpdateRepository = apprenticeshipUpdateRepository;
        }

        public async Task<GetPendingApprenticeshipUpdateResponse> Handle(GetPendingApprenticeshipUpdateRequest message)
        {
            var validationResult = _validator.Validate(message);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var result = await _apprenticeshipUpdateRepository.GetPendingApprenticeshipUpdate(message.ApprenticeshipId);

            return new GetPendingApprenticeshipUpdateResponse
            {
                Data = result == null ? null : MapFrom(result)
            };
        }

        private PendingApprenticeshipUpdatePlaceholder MapFrom(ApprenticeshipUpdate source)
        {
            return new PendingApprenticeshipUpdatePlaceholder
            {
                Id = source.Id,
                ApprenticeshipId = source.ApprenticeshipId,
                Originator = source.Originator,
                FirstName = source.FirstName,
                LastName = source.LastName,
                DateOfBirth = source.DateOfBirth,
                ULN = source.ULN,
                TrainingCode = source.TrainingCode,
                TrainingType = source.TrainingType,
                TrainingName = source.TrainingName,
                Cost = source.Cost,
                StartDate = source.StartDate,
                EndDate = source.EndDate
            };
        }
    }
}
