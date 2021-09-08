using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

using SFA.DAS.Commitments.Application.Commands;
using SFA.DAS.Commitments.Application.Commands.BulkUploadApprenticships;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Encoding;
using ValidationFailReason = SFA.DAS.Commitments.Domain.Entities.Validation.ValidationFailReason;
using SFA.DAS.Learners.Validators;
using SFA.DAS.Reservations.Api.Types;
using SFA.DAS.Commitments.Application.Queries.GetEmailOverlappingApprenticeships;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.BulkUploadApprenticeships
{
    [TestFixture]
    public sealed class WhenCreatingApprenticeships
    {
        private Mock<ICommitmentRepository> _mockCommitmentRespository;
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRespository;
        private BulkUploadApprenticeshipsCommandHandler _handler;
        private BulkUploadApprenticeshipsCommand _exampleValidRequest;
        private List<Apprenticeship> _exampleApprenticships;
        private Mock<IApprenticeshipEvents> _mockApprenticeshipEvents;
        private Mock<IMediator> _mockMediator;
        private Mock<IHistoryRepository> _mockHistoryRepository;
        private Mock<IUlnValidator> _mockUlnValidator;
        private Mock<IAcademicYearValidator> _mockAcademicYearValidator;
        private Mock<ICurrentDateTime> _stubCurrentDateTime;
        private Mock<IReservationsApiClient> _mockReservationApiClient;
        private Mock<IEncodingService> _mockEncodingService;
        private Mock<IV2EventsPublisher> _mockV2EventsPublisher;

        private Commitment _existingCommitment;
        private List<Apprenticeship> _existingApprenticeships;
        private long _legalEntityId = 123456;
        private BulkCreateReservationsResult _bulkCreateReservationsResult =
            new BulkCreateReservationsResult(new List<Guid> {Guid.NewGuid(), Guid.NewGuid()});

        [SetUp]
        public void SetUp()
        {
            _mockApprenticeshipEvents = new Mock<IApprenticeshipEvents>();
            _mockCommitmentRespository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipRespository = new Mock<IApprenticeshipRepository>();
            _mockMediator = new Mock<IMediator>();
            _mockHistoryRepository = new Mock<IHistoryRepository>();
            _mockUlnValidator = new Mock<IUlnValidator>();
            _mockAcademicYearValidator = new Mock<IAcademicYearValidator>();
            _stubCurrentDateTime = new Mock<ICurrentDateTime>();
            _mockReservationApiClient = new Mock<IReservationsApiClient>();
            _mockEncodingService = new Mock<IEncodingService>();
            _mockV2EventsPublisher = new Mock<IV2EventsPublisher>();

            var validator = new BulkUploadApprenticeshipsValidator(new ApprenticeshipValidator(_stubCurrentDateTime.Object, _mockUlnValidator.Object, _mockAcademicYearValidator.Object));

            _handler = new BulkUploadApprenticeshipsCommandHandler(
                _mockCommitmentRespository.Object,
                _mockApprenticeshipRespository.Object,
                validator,
                _mockApprenticeshipEvents.Object,
                Mock.Of<ICommitmentsLogger>(), 
                _mockMediator.Object,
                _mockHistoryRepository.Object,
                _mockReservationApiClient.Object,
                _mockEncodingService.Object,
                _mockV2EventsPublisher.Object);

            _stubCurrentDateTime.Setup(x => x.Now).Returns(new DateTime(2018, 4, 1));

            _exampleApprenticships = new List<Apprenticeship>
            {
                new Apprenticeship { FirstName = "Bob", LastName = "Smith", ULN = "1234567890", Email="bob@test.com", StartDate = new DateTime(2018,5,1), EndDate = DateTime.Now.AddMonths(2)},
                new Apprenticeship { FirstName = "Jane", LastName = "Jones", ULN = "1122334455", Email="jane@test.com", StartDate = new DateTime(2019,3,1), EndDate = new DateTime(2019,9,2)},
            };

            _exampleValidRequest = new BulkUploadApprenticeshipsCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = 111L
                },
                CommitmentId = 123L,
                Apprenticeships = _exampleApprenticships,
                UserId = "User",
                UserName = "Bob"
            };

            _existingApprenticeships = new List<Apprenticeship>();
            _existingCommitment = new Commitment { ProviderId = 111L, EditStatus = EditStatus.ProviderOnly, Apprenticeships = _existingApprenticeships, EmployerAccountId = 987, TransferSenderId = 888 };

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(_existingCommitment);

            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetOverlappingApprenticeshipsRequest>()))
                .ReturnsAsync(new GetOverlappingApprenticeshipsResponse {Data = new List<ApprenticeshipResult>()});

            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetEmailOverlappingApprenticeshipsRequest>()))
            .ReturnsAsync(new Application.Queries.GetEmailOverlappingApprenticeships.GetEmailOverlappingApprenticeshipsResponse { Data = new List<OverlappingEmail>() });

            _mockEncodingService.Setup(x => x.Decode(It.IsAny<string>(), It.IsAny<EncodingType>()))
                .Returns(_legalEntityId);

            _mockReservationApiClient.Setup(x => x.BulkCreateReservations(It.IsAny<long>(), It.IsAny<BulkCreateReservationsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_bulkCreateReservationsResult);
            _mockApprenticeshipRespository.Setup(x => x.BulkUploadApprenticeships(It.IsAny<long>(), It.IsAny<IEnumerable<Apprenticeship>>())).ReturnsAsync(new List<Apprenticeship>());
        }

        [Test]
        public async Task ShouldDecodeLegalEntityPublicHashedIdBeforePassingItOn()
        {
            await _handler.Handle(_exampleValidRequest);

            _mockEncodingService.Verify(
                x => x.Decode(_existingCommitment.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId), Times.Once);
        }

        [Test]
        public async Task ShouldCallReservationsApiClientsBulkCreateEndpointWithCorrectContent()
        {
            await _handler.Handle(_exampleValidRequest);

            _mockReservationApiClient.Verify(
                x => x.BulkCreateReservations(_legalEntityId,
                    It.Is<BulkCreateReservationsRequest>(p => p.Count == _exampleValidRequest.Apprenticeships.Count() && p.TransferSenderId == _existingCommitment.TransferSenderId),
                    It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        public void ShouldThrowInvalidOperationExceptionIfNumberOfReservationsIdsDoesNotMatchNumberOfApprentices(int numberOfReservations)
        {
            _mockReservationApiClient.Setup(x => x.BulkCreateReservations(It.IsAny<long>(),
                It.IsAny<BulkCreateReservationsRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(CreateBulkCreateReservationsResultWithCountOf(numberOfReservations));

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public async Task ShouldNowHaveAReservationIdAgainstEachApprenticeWhenPassedToRepository()
        {
            await _handler.Handle(_exampleValidRequest);

            _mockApprenticeshipRespository.Verify(
                x => x.BulkUploadApprenticeships(It.IsAny<long>(),
                    It.Is<IEnumerable<Apprenticeship>>(p =>
                        p.First().ReservationId != null && p.Last().ReservationId != null)), Times.Once);
        }

        [Test]
        public async Task ShouldCallCommitmentRepository()
        {
            await _handler.Handle(_exampleValidRequest);

            _mockApprenticeshipRespository.Verify(x => x.BulkUploadApprenticeships(It.IsAny<long>(), It.IsAny<IEnumerable<Apprenticeship>>()), Times.Once);
        }

        [Test]
        public async Task ShouldPublishApprenticeshipDeletedEvents()
        {
            await _handler.Handle(_exampleValidRequest);

            _mockApprenticeshipEvents.Verify(x => x.BulkPublishDeletionEvent(_existingCommitment, _existingApprenticeships, "APPRENTICESHIP-DELETED"), Times.Once);
        }

        [Test]
        public async Task ShouldPublishApprenticeshipCreatedEvents()
        {
            var insertedApprenticeships = new List<Apprenticeship> { new Apprenticeship() };
            _mockApprenticeshipRespository.Setup(x => x.BulkUploadApprenticeships(It.IsAny<long>(), It.IsAny<IEnumerable<Apprenticeship>>())).ReturnsAsync(insertedApprenticeships);
            
            await _handler.Handle(_exampleValidRequest);

            _mockApprenticeshipEvents.Verify(x => x.BulkPublishEvent(_existingCommitment, insertedApprenticeships, "APPRENTICESHIP-CREATED"), Times.Once);
        }

        [Test]
        public async Task ShouldPublishV2BulkUploadIntoCohortCompleted()
        {
            var insertedApprenticeships = new List<Apprenticeship> { new Apprenticeship() };
            _mockApprenticeshipRespository.Setup(x => x.BulkUploadApprenticeships(It.IsAny<long>(), It.IsAny<IEnumerable<Apprenticeship>>())).ReturnsAsync(insertedApprenticeships);

            await _handler.Handle(_exampleValidRequest);

            _mockV2EventsPublisher.Verify(x => x.PublishBulkUploadIntoCohortCompleted(_existingCommitment.ProviderId.Value, _existingCommitment.Id, 1), Times.Once);
        }

        [Test]
        public void ShouldThrowExceptionIfEditStatusIncorrect()
        {
            var existingCommitment = new Commitment
            {
                ProviderId = 111L,
                EditStatus = EditStatus.EmployerOnly // Expecting ProviderOnly
            };
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(existingCommitment);

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<UnauthorizedException>();
        }

        [Test]
        public void ShouldThrowExceptionIfCommitmentInIncorrectState()
        {
            var existingCommitment = new Commitment
            {
                ProviderId = 111L,
                EditStatus = EditStatus.ProviderOnly,
                CommitmentStatus = CommitmentStatus.Deleted // Expecting Active or New
            }; 

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(existingCommitment);

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void ShouldThrowExceptionIfCallerIdIsNotOwner()
        {
            var existingCommitment = new Commitment
            {
                ProviderId = 999L, // Expecting 111L
                EditStatus = EditStatus.ProviderOnly,
                CommitmentStatus = CommitmentStatus.Active 
            };

            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync(existingCommitment);

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<UnauthorizedException>();
        }

        [Test]
        public void ShouldThrowExceptionIfCommitmentDoesNotExist()
        {
            _mockCommitmentRespository.Setup(x => x.GetCommitmentById(It.IsAny<long>())).ReturnsAsync((Commitment)null);

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ResourceNotFoundException>();
        }

        [Test]
        public async Task ThenOverlappingApprenticeshipValidationShouldBePerformed()
        {
            _mockApprenticeshipRespository.Setup(x => x.BulkUploadApprenticeships(It.IsAny<long>(), It.IsAny<IEnumerable<Apprenticeship>>())).ReturnsAsync(new List<Apprenticeship>());

            //Act
            await _handler.Handle(_exampleValidRequest);

            //Assert
            _mockMediator.Verify(x => x.SendAsync(It.Is<GetOverlappingApprenticeshipsRequest>(r => 
                r.OverlappingApprenticeshipRequests.Count == 2
                && r.OverlappingApprenticeshipRequests[0].Uln == _exampleApprenticships[0].ULN
                && r.OverlappingApprenticeshipRequests[0].StartDate == _exampleApprenticships[0].StartDate.Value
                && r.OverlappingApprenticeshipRequests[0].EndDate == _exampleApprenticships[0].EndDate.Value
                && r.OverlappingApprenticeshipRequests[1].Uln == _exampleApprenticships[1].ULN
                && r.OverlappingApprenticeshipRequests[1].StartDate == _exampleApprenticships[1].StartDate.Value
                && r.OverlappingApprenticeshipRequests[1].EndDate == _exampleApprenticships[1].EndDate.Value
            )), Times.Once);
        }

        [Test]
        public async Task ThenOverlappingEmailApprenticeshipValidationShouldBePerformed()
        {
            //Arrange
            _mockApprenticeshipRespository.Setup(x => x.BulkUploadApprenticeships(It.IsAny<long>(), It.IsAny<IEnumerable<Apprenticeship>>())).ReturnsAsync(new List<Apprenticeship>());

            //Act
            await _handler.Handle(_exampleValidRequest);

            //Assert
            _mockMediator.Verify(x => x.SendAsync(It.Is<GetEmailOverlappingApprenticeshipsRequest>(r =>
                r.OverlappingEmailApprenticeshipRequests.Count == 2
                && r.OverlappingEmailApprenticeshipRequests[0].Email == _exampleApprenticships[0].Email
                && r.OverlappingEmailApprenticeshipRequests[0].StartDate == _exampleApprenticships[0].StartDate.Value
                && r.OverlappingEmailApprenticeshipRequests[0].EndDate == _exampleApprenticships[0].EndDate.Value
                && r.OverlappingEmailApprenticeshipRequests[1].Email == _exampleApprenticships[1].Email
                && r.OverlappingEmailApprenticeshipRequests[1].StartDate == _exampleApprenticships[1].StartDate.Value
                && r.OverlappingEmailApprenticeshipRequests[1].EndDate == _exampleApprenticships[1].EndDate.Value
            )), Times.Once);
        }

        [Test]
        public void ThenShouldThrowExceptionIfAnyOverlapsExist()
        {
            var app = _exampleValidRequest.Apprenticeships.First();
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetOverlappingApprenticeshipsRequest>()))
                .ReturnsAsync(new GetOverlappingApprenticeshipsResponse { Data = new List<ApprenticeshipResult>
                {
                    new ApprenticeshipResult
                    {
                        Id = app.Id,
                        AgreementStatus = app.AgreementStatus,
                        ValidationFailReason = ValidationFailReason.OverlappingEndDate
                    }
                } });

            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public void ThenShouldThrowExceptionIfAnyEmailOverlapsExist()
        {
            //Arrange
            var app = _exampleValidRequest.Apprenticeships.First();
            _mockMediator.Setup(x => x.SendAsync(It.IsAny<GetEmailOverlappingApprenticeshipsRequest>()))
                .ReturnsAsync(new Application.Queries.GetEmailOverlappingApprenticeships.GetEmailOverlappingApprenticeshipsResponse
                {
                    Data = new List<OverlappingEmail>
                    {
                        new OverlappingEmail
                        {
                            Id = app.Id,
                            OverlapStatus = (OverlapStatus)ValidationFailReason.OverlappingEndDate
                        }
                    }
                });

            //Act
            Func<Task> act = async () => await _handler.Handle(_exampleValidRequest);

            //Assert
            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public async Task ThenHistoryRecordsAreCreated()
        {
            var expectedOriginalCommitmentState = JsonConvert.SerializeObject(_existingCommitment);

            var insertedApprenticeships = new List<Apprenticeship> { new Apprenticeship { Id = 1234, ProviderId = _existingCommitment.ProviderId.Value, EmployerAccountId = _existingCommitment.EmployerAccountId } };
            _mockApprenticeshipRespository.Setup(x => x.BulkUploadApprenticeships(It.IsAny<long>(), It.IsAny<IEnumerable<Apprenticeship>>())).ReturnsAsync(insertedApprenticeships);

            await _handler.Handle(_exampleValidRequest);

            _mockHistoryRepository.Verify(
                x =>
                    x.InsertHistory(
                        It.Is<IEnumerable<HistoryItem>>(
                            y =>
                                y.First().ChangeType == CommitmentChangeType.BulkUploadedApprenticeships.ToString() &&
                                y.First().CommitmentId == _existingCommitment.Id &&
                                y.First().ApprenticeshipId == null &&
                                y.First().OriginalState == expectedOriginalCommitmentState &&
                                y.First().UpdatedByRole == _exampleValidRequest.Caller.CallerType.ToString() &&
                                y.First().UpdatedState == expectedOriginalCommitmentState &&
                                y.First().UserId == _exampleValidRequest.UserId &&
                                y.First().ProviderId == _existingCommitment.ProviderId &&
                                y.First().EmployerAccountId == _existingCommitment.EmployerAccountId &&
                                y.First().UpdatedByName == _exampleValidRequest.UserName)), Times.Once);

            _mockHistoryRepository.Verify(
                x =>
                    x.InsertHistory(
                        It.Is<IEnumerable<HistoryItem>>(
                            y =>
                                y.Last().ChangeType == ApprenticeshipChangeType.Created.ToString() &&
                                y.Last().CommitmentId == null &&
                                y.Last().ApprenticeshipId == insertedApprenticeships[0].Id &&
                                y.Last().OriginalState == null &&
                                y.Last().UpdatedByRole == _exampleValidRequest.Caller.CallerType.ToString() &&
                                y.Last().UpdatedState != null &&
                                y.Last().UserId == _exampleValidRequest.UserId &&
                                y.Last().ProviderId == _existingCommitment.ProviderId &&
                                y.Last().EmployerAccountId == _existingCommitment.EmployerAccountId &&
                                y.Last().UpdatedByName == _exampleValidRequest.UserName)), Times.Once);
        }

        private BulkCreateReservationsResult CreateBulkCreateReservationsResultWithCountOf(int ncount)
        {

            var list = new List<Guid>();
            for (int i = 0; i < ncount; i++)
            {
                list.Add(Guid.NewGuid());
            }

            return new BulkCreateReservationsResult(list);
        }
    }
}