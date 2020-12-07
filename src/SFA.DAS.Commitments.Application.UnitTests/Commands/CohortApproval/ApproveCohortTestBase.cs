using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Entities.Validation;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Messaging.Interfaces;
using AgreementStatus = SFA.DAS.Commitments.Domain.Entities.AgreementStatus;
using CommitmentStatus = SFA.DAS.Commitments.Domain.Entities.CommitmentStatus;
using EditStatus = SFA.DAS.Commitments.Domain.Entities.EditStatus;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CohortApproval
{
    public abstract class ApproveCohortTestBase<T> where T : IAsyncRequest
    {
        protected AbstractValidator<T> Validator;
        protected Mock<ICommitmentRepository> CommitmentRepository;
        protected Mock<IApprenticeshipRepository> ApprenticeshipRepository;
        protected Mock<IApprenticeshipOverlapRules> OverlapRules;
        protected Mock<IEmployerAccountsService> EmployerAccountsService;
        protected Mock<IV2EventsPublisher> V2EventsPublisher;
        protected AsyncRequestHandler<T> Target;
        protected T Command;
        protected Commitment Commitment;
        protected Account Account;

        [Test]
        public void ThenIfTheCommitmentCannotEditedItCannotBeApproved()
        {
            Commitment.EditStatus = EditStatus.Neither;

            Assert.ThrowsAsync<InvalidOperationException>(() => Target.Handle(Command));
        }

        protected void SetUpCommonMocks()
        {
            CommitmentRepository = new Mock<ICommitmentRepository>();
            ApprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            OverlapRules = new Mock<IApprenticeshipOverlapRules>();
            EmployerAccountsService = new Mock<IEmployerAccountsService>();
            V2EventsPublisher = new Mock<IV2EventsPublisher>();
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

        protected Account CreateAccount(long accountId, ApprenticeshipEmployerType apprenticeshipEmployerType)
        {
            return new Account
            {
                Id = accountId,
                ApprenticeshipEmployerType = apprenticeshipEmployerType
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
