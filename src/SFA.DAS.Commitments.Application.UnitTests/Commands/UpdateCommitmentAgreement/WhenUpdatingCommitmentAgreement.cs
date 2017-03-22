using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types.Validation;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using LastAction = SFA.DAS.Commitments.Api.Types.Commitment.Types.LastAction;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateCommitmentAgreement
{
    [TestFixture]
    public sealed class WhenUpdatingCommitmentAgreement
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRespository;
        private UpdateCommitmentAgreementCommandHandler _handler;
        private UpdateCommitmentAgreementCommand _validCommand;
        private Mock<IMediator> _mockMediator;
        private Mock<IApprenticeshipEvents> _mockApprenticeshipEvents;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetOverlappingApprenticeshipsRequest>()))
                .ReturnsAsync(new GetOverlappingApprenticeshipsResponse
                {
                    Data = new List<OverlappingApprenticeship>()
                });

            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipRespository = new Mock<IApprenticeshipRepository>();
            _mockApprenticeshipEvents = new Mock<IApprenticeshipEvents>();
            _handler = new UpdateCommitmentAgreementCommandHandler(
                _mockCommitmentRespository.Object,
                _mockApprenticeshipRespository.Object,
                new ApprenticeshipUpdateRules(), 
                _mockApprenticeshipEvents.Object, 
                Mock.Of<ICommitmentsLogger>(),
                _mockMediator.Object,
                new UpdateCommitmentAgreementCommandValidator());

            _validCommand = new UpdateCommitmentAgreementCommand
            {
                Caller = new Domain.Caller { Id = 444, CallerType = Domain.CallerType.Employer },
                LatestAction = Api.Types.Commitment.Types.LastAction.Amend,
                CommitmentId = 123L,
                LastUpdatedByName = "Test Tester",
                LastUpdatedByEmail = "test@tester.com"
            };
        }

        [Test]
        public void ShouldThrowExceptionIfActionIsNotSetToValidValue()
        {
            _validCommand.LatestAction = (Api.Types.Commitment.Types.LastAction)99;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            act.ShouldThrowExactly<ValidationException>();
        }

        [Test]
        public void ShouldThrowExceptionIfCallerNotSet()
        {
            _validCommand.Caller = null;

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
        [TestCase("あいうえお@example.com")]
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
        public void ShouldThrowAnExceptionIfEmployerTriesToApproveWhenCommitmentNotComplete()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = false, EditStatus = EditStatus.EmployerOnly };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.Caller.Id = 444;
            _validCommand.Caller.CallerType = Domain.CallerType.Employer;
            _validCommand.LatestAction = Api.Types.Commitment.Types.LastAction.Approve;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };
            act.ShouldThrow<InvalidOperationException>().WithMessage("Commitment 123 cannot be approved because apprentice information is incomplete");
        }

        [Test]
        public void ShouldThrowAnExceptionIfProviderTriesToApproveWhenCommitmentNotComplete()
        {
            var commitment = new Commitment { Id = 123L, ProviderId = 333, ProviderCanApproveCommitment = false, EditStatus = EditStatus.ProviderOnly };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.Caller.Id = 333;
            _validCommand.Caller.CallerType = Domain.CallerType.Provider;
            _validCommand.LatestAction = Api.Types.Commitment.Types.LastAction.Approve;

            Func<Task> act = async () => { await _handler.Handle(_validCommand); };
            act.ShouldThrow<InvalidOperationException>().WithMessage("Commitment 123 cannot be approved because apprentice information is incomplete");
        }


        [Test]
        public async Task ThenIfApprovingCommitmentThenMediatorIsCalledToCheckForOverlappingApprenticeships()
        {
            //Arrange
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.LatestAction = LastAction.Approve;

            //Act
            await _handler.Handle(_validCommand);

            //Assert
            _mockMediator.Verify(x => x.SendAsync(It.IsAny<GetOverlappingApprenticeshipsRequest>()), Times.Once);
        }

        [Test]
        public async Task ThenIfNotApprovingCommitmentThenMediatorIsNotCalledToCheckForOverlappingApprenticeships()
        {
            //Arrange
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _validCommand.LatestAction = LastAction.Amend;

            //Act
            await _handler.Handle(_validCommand);

            //Assert
            _mockMediator.Verify(x => x.SendAsync(It.IsAny<GetOverlappingApprenticeshipsRequest>()), Times.Never);
        }

        [Test]
        public void ThenIfApprovingCommitmentThenIfThereAreOverlappingApprenticeshipsThenThrowAnException()
        {
            //Arrange
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetOverlappingApprenticeshipsRequest>()))
                .ReturnsAsync(new GetOverlappingApprenticeshipsResponse
                {
                    Data = new List<OverlappingApprenticeship>
                    {
                        new OverlappingApprenticeship()
                    }
                });

            _validCommand.LatestAction = LastAction.Approve;

            //Act
            Func<Task> act = async () => { await _handler.Handle(_validCommand); };

            //Assert
            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public async Task ThenIfAnApprenticeshipIsUpdatedAnEventIsPublished()
        {
            var commitment = new Commitment { Id = 123L, EmployerAccountId = 444, EmployerCanApproveCommitment = true, EditStatus = EditStatus.EmployerOnly };
            var apprenticeship = new Apprenticeship { AgreementStatus = AgreementStatus.NotAgreed, PaymentStatus = PaymentStatus.PendingApproval, Id = 1234 };
            commitment.Apprenticeships.Add(apprenticeship);

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(commitment);

            var updatedApprenticeship = new Apprenticeship();
            _mockApprenticeshipRespository.Setup(x => x.GetApprenticeship(apprenticeship.Id)).ReturnsAsync(updatedApprenticeship);

            _validCommand.LatestAction = LastAction.Approve;

            await _handler.Handle(_validCommand);

            _mockApprenticeshipEvents.Verify(x => x.PublishEvent(commitment, updatedApprenticeship, "APPRENTICESHIP-AGREEMENT-UPDATED"), Times.Once);
        }
    }
}
