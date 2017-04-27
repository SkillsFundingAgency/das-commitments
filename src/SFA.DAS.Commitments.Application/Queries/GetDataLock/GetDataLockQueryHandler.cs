using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Application.Queries.GetDataLock
{
    public sealed class GetDataLockQueryHandler : IAsyncRequestHandler<GetDataLockRequest, GetDataLockResponse>
    {
        private readonly AbstractValidator<GetDataLockRequest> _validator;
        private readonly IDataLockRepository _dataLockRepository;

        public GetDataLockQueryHandler(AbstractValidator<GetDataLockRequest> validator, IDataLockRepository dataLockRepository)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            if (dataLockRepository == null)
                throw new ArgumentNullException(nameof(IDataLockRepository));

            _validator = validator;
            _dataLockRepository = dataLockRepository;
        }

        public async Task<GetDataLockResponse> Handle(GetDataLockRequest message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var data = await _dataLockRepository.GetDataLock(message.DataLockEventId);

            //todo: assert datalock belongs to apprenticeship

            return new GetDataLockResponse
            {
                Data = MapFrom(data)
            };
        }

        private Api.Types.DataLock.DataLockStatus MapFrom(DataLockStatus source)
        {
            return new Api.Types.DataLock.DataLockStatus
            {
                ApprenticeshipId  = source.ApprenticeshipId,
                DataLockEventDatetime  = source.DataLockEventDatetime,
                DataLockEventId = source.DataLockEventId,
                ErrorCode = (Api.Types.DataLock.Types.DataLockErrorCode) source.ErrorCode,
                IlrActualStartDate = source.IlrActualStartDate,
                IlrEffectiveFromDate = source.IlrEffectiveFromDate,
                IlrTotalCost = source.IlrTotalCost,
                IlrTrainingCourseCode = source.IlrTrainingCourseCode,
                IlrTrainingType = (TrainingType) source.IlrTrainingType,
                PriceEpisodeIdentifier = source.PriceEpisodeIdentifier,
                Status = (Api.Types.DataLock.Types.Status) source.Status,
                TriageStatus = (Api.Types.DataLock.Types.TriageStatus) source.TriageStatus,
            };
        }
    }
}
