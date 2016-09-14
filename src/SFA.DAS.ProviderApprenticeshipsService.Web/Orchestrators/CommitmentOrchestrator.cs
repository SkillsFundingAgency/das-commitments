using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.ProviderApprenticeshipsService.Application.Commands.CreateApprenticeship;
using SFA.DAS.ProviderApprenticeshipsService.Application.Commands.SubmitCommitment;
using SFA.DAS.ProviderApprenticeshipsService.Application.Commands.UpdateApprenticeship;
using SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetApprenticeship;
using SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetCommitment;
using SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetCommitments;
using SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetStandards;
using SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetTasks;
using SFA.DAS.ProviderApprenticeshipsService.Web.Models;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.Orchestrators
{
    public class CommitmentOrchestrator
    {
        private readonly IMediator _mediator;

        public CommitmentOrchestrator(IMediator mediator)
        {
            if (mediator == null)
                throw new ArgumentNullException(nameof(mediator));
            _mediator = mediator;
        }

        public async Task<CommitmentListViewModel> GetAll(long providerId)
        {
            var data = await _mediator.SendAsync(new GetCommitmentsQueryRequest
            {
                ProviderId = providerId
            });

            var tasks = await _mediator.SendAsync(new GetTasksQueryRequest
            {
                ProviderId = providerId
            });

            return new CommitmentListViewModel
            {
                ProviderId = providerId,
                NumberOfTasks = tasks.Tasks.Count,
                Commitments = data.Commitments
            };
        }

        public async Task<CommitmentViewModel> Get(long providerId, long commitmentId)
        {
            var data = await _mediator.SendAsync(new GetCommitmentQueryRequest
            {
                ProviderId = providerId,
                CommitmentId = commitmentId
            });

            return new CommitmentViewModel
            {
                Commitment = data.Commitment
            };
        }

        public async Task<ExtendedApprenticeshipViewModel> GetApprenticeship(long providerId, long commitmentId, long apprenticeshipId)
        {
            var data = await _mediator.SendAsync(new GetApprenticeshipQueryRequest
            {
                ProviderId = providerId,
                CommitmentId = commitmentId,
                AppenticeshipId = apprenticeshipId
            });

            var standards = await _mediator.SendAsync(new GetStandardsQueryRequest());

            var apprenticeship = MapFrom(data.Apprenticeship);

            apprenticeship.ProviderId = providerId;

            return new ExtendedApprenticeshipViewModel
            {
                Apprenticeship = apprenticeship,
                Standards = standards.Standards
            };
        }

        public async Task<ExtendedApprenticeshipViewModel> GetApprenticeship(long providerId, long commitmentId)
        {
            var standards = await _mediator.SendAsync(new GetStandardsQueryRequest());

            var apprenticeship = new ApprenticeshipViewModel
            {
                ProviderId = providerId,
                CommitmentId = commitmentId,
            };

            return new ExtendedApprenticeshipViewModel
            {
                Apprenticeship = apprenticeship,
                Standards = standards.Standards
            };
        }

        public async Task UpdateApprenticeship(ApprenticeshipViewModel apprenticeship)
        {
            await _mediator.SendAsync(new UpdateApprenticeshipCommand
            {
                ProviderId = apprenticeship.ProviderId,
                Apprenticeship = MapFrom(apprenticeship)
            });
        }

        public async Task CreateApprenticeship(ApprenticeshipViewModel apprenticeship)
        {
            await _mediator.SendAsync(new CreateApprenticeshipCommand
            {
                ProviderId = apprenticeship.ProviderId,
                Apprenticeship = MapFrom(apprenticeship)
            });
        }

        public async Task SubmitApprenticeship(SubmitCommitmentModel model)
        {
            await _mediator.SendAsync(new SubmitCommitmentCommand
            {
                ProviderId = model.ProviderId,
                CommitmentId = model.CommitmentId,
                Message = model.Message
            });
        }

        private ApprenticeshipViewModel MapFrom(Apprenticeship apprenticeship)
        {
            return new ApprenticeshipViewModel
            {
                Id = apprenticeship.Id,
                CommitmentId = apprenticeship.CommitmentId,
                FirstName = apprenticeship.FirstName,
                LastName = apprenticeship.LastName,
                ULN = apprenticeship.ULN,
                TrainingId = apprenticeship.TrainingId,
                Cost = apprenticeship.Cost,
                StartMonth = apprenticeship.StartDate?.Month, 
                StartYear = apprenticeship.StartDate?.Year,
                EndMonth = apprenticeship.EndDate?.Month,
                EndYear = apprenticeship.EndDate?.Year,
                Status = apprenticeship.Status.ToString(),
                AgreementStatus = apprenticeship.AgreementStatus.ToString()
            };
        }

        private Apprenticeship MapFrom(ApprenticeshipViewModel viewModel)
        {
            return new Apprenticeship
            {
                Id = viewModel.Id,
                CommitmentId = viewModel.CommitmentId,
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                ULN = viewModel.ULN,
                TrainingId = viewModel.TrainingId,
                Cost = viewModel.Cost,
                StartDate = GetDateTime(viewModel.StartMonth, viewModel.StartYear),
                EndDate = GetDateTime(viewModel.EndMonth, viewModel.EndYear)
            };
        }

        private DateTime? GetDateTime(int? month, int? year)
        {
            if (month.HasValue && year.HasValue)
                return new DateTime(year.Value, month.Value, 1);

            return null;
        }
    }
}