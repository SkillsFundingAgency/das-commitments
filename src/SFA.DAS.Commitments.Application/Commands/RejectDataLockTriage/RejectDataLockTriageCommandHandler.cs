using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Extensions;

namespace SFA.DAS.Commitments.Application.Commands.RejectDataLockTriage
{
    public class RejectDataLockTriageCommandHandler : AsyncRequestHandler<RejectDataLockTriageCommand>
    {
        private readonly AbstractValidator<RejectDataLockTriageCommand> _validator;
        private readonly IDataLockRepository _dataLockRepository;

        public RejectDataLockTriageCommandHandler(
            AbstractValidator<RejectDataLockTriageCommand> validator,
            IDataLockRepository dataLockRepository,
            IApprenticeshipRepository apprenticeshipRepository,
            ICommitmentRepository commitmentRepository)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(AbstractValidator<RejectDataLockTriageCommand>));
            if (dataLockRepository == null)
                throw new ArgumentNullException(nameof(IDataLockRepository));
            if (apprenticeshipRepository == null)
                throw new ArgumentNullException(nameof(IApprenticeshipRepository));
            if(commitmentRepository == null)
                throw new ArgumentNullException(nameof(ICommitmentRepository));

            _validator = validator;
            _dataLockRepository = dataLockRepository;
        }
        protected override async Task HandleCore(RejectDataLockTriageCommand command)
        {
            var validationResult = _validator.Validate(command);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var datalocks = await _dataLockRepository.GetDataLocks(command.ApprenticeshipId);

            var dataLockPriceErrors = datalocks
                .Where(DataLockExtensions.UnHandled)
                .Where(DataLockExtensions.IsPriceOnly)
                .Where(x => x.TriageStatus == TriageStatus.Change)
                .ToList();

            if (!dataLockPriceErrors.Any())
                return;
           
            await _dataLockRepository.UpdateDataLockTriageStatus(
                dataLockPriceErrors.Select(m => m.DataLockEventId),
                TriageStatus.Unknown);
        }
    }
}