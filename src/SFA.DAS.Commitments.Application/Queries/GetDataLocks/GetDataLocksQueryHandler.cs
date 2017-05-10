using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

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
                Data = MapFrom(data)
            };
        }

        private IList<Api.Types.DataLock.DataLockStatus> MapFrom(IList<DataLockStatus> sourceList)
        {
            return sourceList.Select(source => new Api.Types.DataLock.DataLockStatus
            {
                ApprenticeshipId = source.ApprenticeshipId,
                DataLockEventDatetime = source.DataLockEventDatetime,
                DataLockEventId = source.DataLockEventId,
                ErrorCode = (Api.Types.DataLock.Types.DataLockErrorCode) source.ErrorCode,
                IlrActualStartDate = source.IlrActualStartDate,
                IlrEffectiveFromDate = source.IlrEffectiveFromDate,
                IlrTotalCost = source.IlrTotalCost,
                IlrTrainingCourseCode = source.IlrTrainingCourseCode,
                IlrTrainingType = (TrainingType) source.IlrTrainingType,
                PriceEpisodeIdentifier = source.PriceEpisodeIdentifier,
                Status = (Api.Types.DataLock.Types.Status) source.Status,
                TriageStatus = (Api.Types.DataLock.Types.TriageStatus) source.TriageStatus
            }).ToList();
        }
    }
}
