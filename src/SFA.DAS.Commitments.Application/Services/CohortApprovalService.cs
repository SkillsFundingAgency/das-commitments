using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Newtonsoft.Json;
using SFA.DAS.Commitments.Application.Commands.SetPaymentOrder;
using SFA.DAS.Commitments.Application.Extensions;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.Services
{
    internal class CohortApprovalService
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IMediator _mediator;
        private readonly OverlappingApprenticeshipService _overlappingApprenticeshipService;
        private readonly ApprenticeshipEventsService _apprenticeshipEventsService;
        private readonly IApprenticeshipInfoService _apprenticeshipInfoService;
        private readonly ICommitmentsLogger _logger;

        internal CohortApprovalService(IApprenticeshipRepository apprenticeshipRepository,
            IApprenticeshipOverlapRules overlapRules,
            ICurrentDateTime currentDateTime,
            ICommitmentRepository commitmentRepository,
            IApprenticeshipEventsList apprenticeshipEventsList,
            IApprenticeshipEventsPublisher apprenticeshipEventsPublisher,
            IMediator mediator,
            ICommitmentsLogger logger,
            IApprenticeshipInfoService apprenticeshipInfoService,
            IV2EventsPublisher v2EventsPublisher)
        {
            _apprenticeshipRepository = apprenticeshipRepository;
            _currentDateTime = currentDateTime;
            _commitmentRepository = commitmentRepository;
            _mediator = mediator;
            _logger = logger;
            _apprenticeshipInfoService = apprenticeshipInfoService;
            _overlappingApprenticeshipService = new OverlappingApprenticeshipService(apprenticeshipRepository, overlapRules);
            _apprenticeshipEventsService = new ApprenticeshipEventsService(apprenticeshipEventsList,
                apprenticeshipEventsPublisher,
                apprenticeshipRepository,
                logger, v2EventsPublisher);
        }

        //not sure why we can't dependency inject the message publisher
        internal async Task CreateTransferRequest(Commitment commitment, IMessagePublisher messagePublisher)
        {
            decimal totalCost = 0;
            var totalFundingCap = 0;

            foreach (var apprenticeship in commitment.Apprenticeships)
            {
                var course = await _apprenticeshipInfoService.GetTrainingProgram(apprenticeship.TrainingCode);
                var cap = course.FundingCapOn(apprenticeship.StartDate.Value);
                totalFundingCap += cap;
                totalCost += apprenticeship.Cost.Value < cap ? apprenticeship.Cost.Value : cap;
            }

            var trainingCourseSummaries = TrainingCourseSummaries(commitment);

            var transferRequestId = await _commitmentRepository.StartTransferRequestApproval(commitment.Id, totalCost, totalFundingCap, trainingCourseSummaries);

            await PublishCommitmentRequiresApprovalByTransferSenderEventMessage(messagePublisher, commitment, transferRequestId, totalCost);

            commitment.TransferApprovalStatus = TransferApprovalStatus.Pending;
        }

        private List<TrainingCourseSummary> TrainingCourseSummaries(Commitment commitment)
        {
            var apprenticeships = commitment.Apprenticeships ?? new List<Apprenticeship>();

            var grouped = apprenticeships.GroupBy(l => l.TrainingCode).Select(cl =>
                new TrainingCourseSummary
                {
                    CourseTitle = cl.First().TrainingName,
                    ApprenticeshipCount = cl.Count()
                });

            return grouped.ToList();
        }

        private async Task PublishCommitmentRequiresApprovalByTransferSenderEventMessage(IMessagePublisher messagePublisher, Commitment commitment, long transferRequestId, decimal totalCost)
        {
            var senderMessage = new CohortApprovalByTransferSenderRequested(transferRequestId, commitment.EmployerAccountId,
                commitment.Id, commitment.TransferSenderId.Value, totalCost);
            await messagePublisher.PublishAsync(senderMessage);
        }

        internal async Task CreatePriceHistory(Commitment commitment)
        {
            await _apprenticeshipRepository.CreatePriceHistoryForApprenticeshipsInCommitment(commitment.Id);

            foreach (var apprenticeship in commitment.Apprenticeships)
            {
                apprenticeship.PriceHistory = new List<PriceHistory>
                {
                    new PriceHistory { ApprenticeshipId = apprenticeship.Id, Cost = apprenticeship.Cost.Value, FromDate = apprenticeship.StartDate.Value }
                };
            }
        }
    }
}
