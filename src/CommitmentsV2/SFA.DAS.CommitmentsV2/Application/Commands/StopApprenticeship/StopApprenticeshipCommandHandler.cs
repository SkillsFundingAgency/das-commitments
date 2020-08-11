using MediatR;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship
{
    public class StopApprenticeshipCommandHandler : AsyncRequestHandler<StopApprenticeshipCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IApprenticeshipDomainService _apprenticeshipService;
        private readonly ICurrentDateTime _currentDate;

        public StopApprenticeshipCommandHandler(
            IApprenticeshipDomainService apprenticeshipService,
            Lazy<ProviderCommitmentsDbContext> dbContext,
            ICurrentDateTime currentDate)
        {
            _apprenticeshipService = apprenticeshipService;
            _dbContext = dbContext;
            _currentDate = currentDate;
        }

        protected async override Task Handle(StopApprenticeshipCommand request, CancellationToken cancellationToken)
        {
            // check validation occurs on the validator 
            // does it throw with empty requests

            var apprenticeship = await _apprenticeshipService.GetApprenticeshipById(request.ApprenticeshipId);

            if (apprenticeship.PaymentStatus == Types.PaymentStatus.Completed)
            {
                throw new DomainException(nameof(apprenticeship.PaymentStatus), "Apprenticeship Payment status already set to completed. Unable to stop apprenticeship");
            }
            
            if (apprenticeship.Cohort.EmployerAccountId != request.AccountId)
            {
                throw new DomainException(nameof(request.AccountId), $"Employer {request.AccountId} not authorised to access commitment {apprenticeship.Cohort.Id}, expected employer {apprenticeship.Cohort.EmployerAccountId}");
            }

            ValidateChangeDateForStop(request.StopDate, apprenticeship);

            apprenticeship.StopApprenticeship(request.StopDate, request.MadeRedundant, request.UserInfo);

            // Resolve Data Locks as per previous command handler

            // publish events?
        }

        private void ValidateChangeDateForStop(DateTime dateOfChange, Apprenticeship apprenticeship)
        {
            if (apprenticeship == null) throw new ArgumentException(nameof(apprenticeship));

            if (apprenticeship.IsWaitingToStart(_currentDate))
            {
                if (dateOfChange.Date != apprenticeship.StartDate.Value.Date)
                    throw new DomainException(nameof(dateOfChange), "Invalid Date of Change. Date should be value of start date if training has not started.");
            }
            else
            {
                if (dateOfChange.Date > _currentDate.UtcNow.Date)
                    throw new DomainException(nameof(dateOfChange), "Invalid Date of Change. Date cannot be in the future.");

                if (dateOfChange.Date < apprenticeship.StartDate.Value.Date)
                    throw new DomainException(nameof(dateOfChange), "Invalid Date of Change. Date cannot be before the training start date.");
            }
        }
    }
}
