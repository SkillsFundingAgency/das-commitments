using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Entities.Validation;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CohortApproval
{
    public abstract class ApproveCohortTestBase<T> where T : IAsyncRequest
    {
        protected AbstractValidator<T> Validator;
        protected Mock<ICommitmentRepository> CommitmentRepository;
        protected Mock<IApprenticeshipRepository> ApprenticeshipRepository;
        protected Mock<IApprenticeshipOverlapRules> OverlapRules;
        protected Mock<ICurrentDateTime> CurrentDateTime;
        protected Mock<IHistoryRepository> HistoryRepository;
        protected Mock<IApprenticeshipEventsList> ApprenticeshipEventsList;
        protected Mock<IApprenticeshipEventsPublisher> ApprenticeshipEventsPublisher;
        protected Mock<IMediator> Mediator;
        protected Mock<IMessagePublisher> MessagePublisher;
        protected AsyncRequestHandler<T> Target;
        protected T Command;
        protected Commitment Commitment;

        [Test]
        public void ThenIfTheCommitmentIsDeletedItCannotBeApproved()
        {
            Commitment.CommitmentStatus = CommitmentStatus.Deleted;

            Assert.ThrowsAsync<InvalidOperationException>(() => Target.Handle(Command));
        }

        [Test]
        public void ThenIfTheCommitmentCannotEditedItCannotBeApproved()
        {
            Commitment.EditStatus = EditStatus.Neither;

            Assert.ThrowsAsync<InvalidOperationException>(() => Target.Handle(Command));
        }

        [Test]
        public void ThenIfTheCommitmentHasOverlappingApprenticeshipsItCannotBeApproved()
        {
            var apprenticeship = Commitment.Apprenticeships.Last();
            var apprenticeshipResult = new ApprenticeshipResult { Uln = apprenticeship.ULN };
            ApprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.Is<IEnumerable<string>>(y => y.First() == Commitment.Apprenticeships.First().ULN && y.Last() == Commitment.Apprenticeships.Last().ULN))).ReturnsAsync(new List<ApprenticeshipResult> { apprenticeshipResult });
            OverlapRules.Setup(x => x.DetermineOverlap(It.Is<ApprenticeshipOverlapValidationRequest>(r => r.Uln == apprenticeship.ULN && r.ApprenticeshipId == apprenticeship.Id && r.StartDate == apprenticeship.StartDate.Value && r.EndDate == apprenticeship.EndDate.Value), apprenticeshipResult)).Returns(ValidationFailReason.OverlappingEndDate);

            Assert.ThrowsAsync<ValidationException>(() => Target.Handle(Command));
        }

        protected bool VerifyHistoryItem(HistoryItem historyItem, CommitmentChangeType changeType, string userId, string lastUpdatedByName, CallerType callerType)
        {
            return historyItem.ChangeType == changeType.ToString() &&
                   historyItem.TrackedObject == Commitment &&
                   historyItem.CommitmentId == Commitment.Id &&
                   historyItem.UpdatedByRole == callerType.ToString() &&
                   historyItem.UserId == userId &&
                   historyItem.ProviderId == Commitment.ProviderId &&
                   historyItem.EmployerAccountId == Commitment.EmployerAccountId &&
                   historyItem.UpdatedByName == lastUpdatedByName;
        }

        protected void SetUpCommonMocks()
        {
            CommitmentRepository = new Mock<ICommitmentRepository>();
            ApprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            OverlapRules = new Mock<IApprenticeshipOverlapRules>();
            CurrentDateTime = new Mock<ICurrentDateTime>();
            HistoryRepository = new Mock<IHistoryRepository>();
            ApprenticeshipEventsList = new Mock<IApprenticeshipEventsList>();
            ApprenticeshipEventsPublisher = new Mock<IApprenticeshipEventsPublisher>();
            Mediator = new Mock<IMediator>();
            MessagePublisher = new Mock<IMessagePublisher>();
        }

        protected Commitment CreateCommitment(long commitmentId, long employerAccountId, long providerId, long? transferSenderId = null, string transferSenderName = null)
        {
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship {TrainingCode = "ABC", TrainingName = "TestTraining", ULN = "1233435", Id = 1, StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month, 25), EndDate = DateTime.Now.AddYears(1), AgreementStatus = AgreementStatus.NotAgreed, Cost = 2347 },
                new Apprenticeship {TrainingCode = "ABC", TrainingName = "TestTraining", ULN = "894567645", Id = 2, StartDate = DateTime.Now.AddYears(-1), EndDate = DateTime.Now.AddYears(2), AgreementStatus = AgreementStatus.NotAgreed, Cost = 23812}
            };
            return new Commitment
            {
                CommitmentStatus = CommitmentStatus.New,
                EditStatus = EditStatus.EmployerOnly,
                Id = commitmentId,
                EmployerAccountId = employerAccountId,
                EmployerCanApproveCommitment = true,
                ProviderCanApproveCommitment = true,
                Apprenticeships = apprenticeships,
                ProviderId = providerId,
                TransferSenderId = transferSenderId,
                TransferSenderName = transferSenderName
            };
        }

        protected void SetupSuccessfulOverlapCheck()
        {
            var apprenticeship = Commitment.Apprenticeships.First();
            var apprenticeshipResult = new ApprenticeshipResult { Uln = apprenticeship.ULN };
            ApprenticeshipRepository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<ApprenticeshipResult> { apprenticeshipResult });
            OverlapRules.Setup(x => x.DetermineOverlap(It.Is<ApprenticeshipOverlapValidationRequest>(r => r.Uln == apprenticeship.ULN && r.ApprenticeshipId == apprenticeship.Id && r.StartDate == apprenticeship.StartDate.Value && r.EndDate == apprenticeship.EndDate.Value), apprenticeshipResult)).Returns(ValidationFailReason.None);
        }
    }
}
