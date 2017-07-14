using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Queries.GetDataLocks
{
    public sealed class GetDataLocksQueryHandler : IAsyncRequestHandler<GetDataLocksRequest, GetDataLocksResponse>
    {
        private readonly AbstractValidator<GetDataLocksRequest> _validator;
        private readonly IDataLockRepository _dataLockRepository;

        public GetDataLocksQueryHandler(AbstractValidator<GetDataLocksRequest> validator, IDataLockRepository dataLockRepository)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            if (dataLockRepository == null)
                throw new ArgumentNullException(nameof(IDataLockRepository));

            _validator = validator;
            _dataLockRepository = dataLockRepository;
        }

        public async Task<GetDataLocksResponse> Handle(GetDataLocksRequest message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var data = await _dataLockRepository.GetDataLocks(message.ApprenticeshipId);

            return new GetDataLocksResponse
            {
                Data = data
            };
        }
    }
}
