using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Messaging.Interfaces;
using AgreementStatus = SFA.DAS.Commitments.Domain.Entities.AgreementStatus;
using CommitmentStatus = SFA.DAS.Commitments.Domain.Entities.CommitmentStatus;
using EditStatus = SFA.DAS.Commitments.Domain.Entities.EditStatus;
using LastAction = SFA.DAS.Commitments.Domain.Entities.LastAction;
using Message = SFA.DAS.Commitments.Domain.Entities.Message;
using PaymentStatus = SFA.DAS.Commitments.Domain.Entities.PaymentStatus;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateCommitmentAgreement
{
    [TestFixture]
    public sealed class WhenUpdatingCommitmentAgreement
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRespository;
        private UpdateCommitmentAgreementCommandHandler _handler;
        private UpdateCommitmentAgreementCommand _validCommand;
        private Mock<IApprenticeshipEventsList> _mockApprenticeshipEventsList;
        private Mock<IApprenticeshipEventsPublisher> _mockApprenticeshipEventsPublisher;
        private Mock<IHistoryRepository> _mockHistoryRepository;
        private Mock<IMessagePublisher> _messagePublisher;
        private Mock<INotificationsPublisher> _notificationsPublisher;
        private Mock<IV2EventsPublisher> V2EventsPublisher;

        [SetUp]
        public void Setup()
        {
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipRespository = new Mock<IApprenticeshipRepository>();
            _mockApprenticeshipRespository.Setup(x => x.GetActiveApprenticeshipsByUlns(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<ApprenticeshipResult>());
            _mockApprenticeshipEventsList = new Mock<IApprenticeshipEventsList>();
            _mockApprenticeshipEventsPublisher = new Mock<IApprenticeshipEventsPublisher>();
            _mockHistoryRepository = new Mock<IHistoryRepository>();
            _messagePublisher = new Mock<IMessagePublisher>();
            _notificationsPublisher = new Mock<INotificationsPublisher>();

            V2EventsPublisher = new Mock<IV2EventsPublisher>();
            V2EventsPublisher.Setup(x => x.SendProviderSendCohortCommand(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<UserInfo>()))
                .Returns(Task.CompletedTask);
            
            _handler = new UpdateCommitmentAgreementCommandHandler(
                _mockCommitmentRespository.Object,
                _mockApprenticeshipRespository.Object,
                new ApprenticeshipUpdateRules(), 
                Mock.Of<ICommitmentsLogger>(),
                new UpdateCommitmentAgreementCommandValidator(),
                _mockApprenticeshipEventsList.Object,
                _mockApprenticeshipEventsPublisher.Object,
                _mockHistoryRepository.Object,
                _messagePublisher.Object,
                _notificationsPublisher.Object,
                V2EventsPublisher.Object);

            _validCommand = new UpdateCommitmentAgreementCommand
            {
                Caller = new Domain.Caller { Id = 444, CallerType = Domain.CallerType.Provider },
                LatestAction = LastAction.Amend,
                CommitmentId = 123L,
                LastUpdatedByName = "Test Tester",
                LastUpdatedByEmail = "test@tester.com"
            };
        }

        [Test]
        public void ShouldThrowExceptionIfActionIsNotSetToValidValue()
        {
            _validCommand.LatestAction = (Domain.Entities.LastAction)99;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            act.ShouldThrowExactly<ValidationException>();
        }

        [Test]
        public void ShouldThrowExceptionIfCallerIdNotSet()
        {
            _validCommand.Caller.Id = 0;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            act.ShouldThrowExactly<ValidationException>();
        }

        [Test]
        public void ShouldThrowExceptionIfCallerTypeNotValid()
        {
            _validCommand.Caller.CallerType = (CallerType)99;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            act.ShouldThrowExactly<ValidationException>();
        }

        [Test]
        public void ShouldThrowExceptionIfCommitmentIdIsInvalid()
        {
            _validCommand.CommitmentId = 0;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            act.ShouldThrowExactly<ValidationException>();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void ShouldThrowExceptionIfLastUpdatedByNameIsNotSet(string value)
        {
            _validCommand.LastUpdatedByName = value;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            act.ShouldThrowExactly<ValidationException>();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        [TestCase("ffdsfdfdsf")]
        [TestCase("#@%^%#$@#$@#.com")]
        [TestCase("@example.com")]
        [TestCase("Joe Smith <email @example.com>")]
        [TestCase("email.example.com")]
        [TestCase("email@example @example.com")]
        [TestCase(".email @example.com")]
        [TestCase("email.@example.com")]
        [TestCase("email..email @example.com")]
        [TestCase("email@example.com (Joe Smith)")]
        [TestCase("email @example")]
        [TestCase("email@-example.com")]
        //[TestCase("email@example.web")] -- This is being accepted by regex
        //[TestCase("email@111.222.333.44444")] -- This is being accepted by regex
        [TestCase("email @example..com")]
        [TestCase("Abc..123@example.com")]
        [TestCase("“(),:;<>[\\] @example.comjust\"not\"right @example.com")]
        [TestCase("this\\ is'really'not\\allowed @example.com")]
        public void ShouldThrowExceptionIfLastUpdatedByEmailIsNotValid(string value)
        {
            _validCommand.LastUpdatedByEmail = value;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            act.ShouldThrowExactly<ValidationException>();
        }


        [Test]
        public async Task ThenIfCallerIsProviderAndLastActionIsNone_NotifyProviderAmendedCohortIsNotCalled()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.ProviderOnly, ProviderId = 325 };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.LatestAction = LastAction.None;
            _validCommand.Caller.CallerType = CallerType.Provider;
            _validCommand.Caller.Id = 325;

            await _handler.Handle(_validCommand);

            _notificationsPublisher.Verify(x => x.ProviderAmendedCohort(It.IsAny<Commitment>()), Times.Never);
        }

        [Test]
        public async Task IfUpdatedByProvider_ThenAProviderSendCohortCommandIsSent()
        {
            _validCommand.Caller = new Caller
            {
                CallerType = CallerType.Provider,
                Id = 325
            };
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.ProviderOnly, ProviderId = 325 };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            await _handler.Handle(_validCommand);
            V2EventsPublisher.Verify(x => x.SendProviderSendCohortCommand(_validCommand.CommitmentId,
                It.Is<string>(m => m == _validCommand.Message),
                It.Is<UserInfo>(u =>
                    u.UserId == _validCommand.UserId &&
                    u.UserDisplayName == _validCommand.LastUpdatedByName &&
                    u.UserEmail == _validCommand.LastUpdatedByEmail)));
        }

        [Test]
        public void IfUpdatedByEmployer_ThenThrows()
        {
            _validCommand.Caller.CallerType = CallerType.Employer;
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly, ProviderId = 325 };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(_validCommand));
        }

        [Test]
        public async Task If_CohortIsAChangePartyRequest_Then_CohortWithChangeOfPartyRequestUpdatedEventIsPublished()
        {
            _validCommand.Caller = new Caller
            {
                CallerType = CallerType.Provider,
                Id = 325
            };
            
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.ProviderOnly, ProviderId = 325, ChangeOfPartyRequestId = 222 };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            await _handler.Handle(_validCommand);

            V2EventsPublisher.Verify(x => x.PublishCohortWithChangeOfPartyUpdatedEvent(_validCommand.CommitmentId,
                It.Is<UserInfo>(u =>
                    u.UserId == _validCommand.UserId &&
                    u.UserDisplayName == _validCommand.LastUpdatedByName &&
                    u.UserEmail == _validCommand.LastUpdatedByEmail)));
        }
    }
}
