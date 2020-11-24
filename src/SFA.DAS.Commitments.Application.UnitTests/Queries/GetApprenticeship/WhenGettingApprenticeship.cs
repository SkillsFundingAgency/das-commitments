using NUnit.Framework;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Data;
using Moq;
using SFA.DAS.Commitments.Domain;
using FluentAssertions;
using System;
using AutoFixture;
using FluentValidation;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetApprenticeship
{
    [TestFixture]
    public sealed class WhenGettingApprenticeship
    {
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRespository;
        private GetApprenticeshipQueryHandler _handler;
        private GetApprenticeshipRequest _exampleValidRequest;
        private Apprenticeship _fakeRepositoryApprenticeship;

        [SetUp]
        public void SetUp()
        {
            _mockApprenticeshipRespository = new Mock<IApprenticeshipRepository>();
            _handler = new GetApprenticeshipQueryHandler(_mockApprenticeshipRespository.Object, new GetApprenticeshipValidator());

            var dataFixture = new Fixture();
            _fakeRepositoryApprenticeship = dataFixture.Build<Apprenticeship>().Create();
            
            _exampleValidRequest = new GetApprenticeshipRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = _fakeRepositoryApprenticeship.EmployerAccountId
                },
                ApprenticeshipId = _fakeRepositoryApprenticeship.Id
            };
        }

        [Test]
        public async Task ThenTheCommitmentRepositoryIsCalled()
        {
            await _handler.Handle(_exampleValidRequest);

            _mockApprenticeshipRespository.Verify(x => x.GetApprenticeship(It.IsAny<long>()), Times.Once);
        }

        [Test]
        public async Task ThenShouldReturnAnApprenticeshipInResponse()
        {
            _mockApprenticeshipRespository.Setup(x => x.GetApprenticeship(It.IsAny<long>())).ReturnsAsync(_fakeRepositoryApprenticeship);

            var response = await _handler.Handle(_exampleValidRequest);

            response.Data.Id.Should().Be(_fakeRepositoryApprenticeship.Id);
            response.Data.FirstName.Should().Be(_fakeRepositoryApprenticeship.FirstName);
            response.Data.LastName.Should().Be(_fakeRepositoryApprenticeship.LastName);
        }

        [Test]
        public void ThenIfApprenticeshipIdIsZeroItThrowsAnInvalidRequestException()
        {
            Func<Task> act = async () => await _handler.Handle(new GetApprenticeshipRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = 1
                }
            });
            act.ShouldThrow<ValidationException>();
        }

        [Test]
        public async Task ThenReturnsAResponseWithNullIfTheCommitmentIsNotFound()
        {
            _mockApprenticeshipRespository.Setup(x => x.GetApprenticeship(It.IsAny<long>())).ReturnsAsync(default(Apprenticeship));

            var response = await _handler.Handle(_exampleValidRequest);

            response.Data.Should().BeNull();
        }

        [Test]
        public void ThenIfAnAccountIdIsProvidedThatDoesntMatchTheApprenticeshipThrowsAnException()
        {
            _mockApprenticeshipRespository.Setup(x => x.GetApprenticeship(It.IsAny<long>())).ReturnsAsync(_fakeRepositoryApprenticeship);

            var employerId = _fakeRepositoryApprenticeship.EmployerAccountId++;

            Func<Task> act = async () => await _handler.Handle(new GetApprenticeshipRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = employerId
                },
                ApprenticeshipId = _fakeRepositoryApprenticeship.Id
            });

            act.ShouldThrow<UnauthorizedException>().WithMessage($"Employer {employerId} not authorised to access apprenticeship {_fakeRepositoryApprenticeship.Id}, expected employer {_fakeRepositoryApprenticeship.EmployerAccountId}");
        }

        [Test]
        public void ThenIfAProviderIdIsProvidedThatDoesntMatchTheApprenticeshipThrowsAnException()
        {
            _mockApprenticeshipRespository.Setup(x => x.GetApprenticeship(It.IsAny<long>())).ReturnsAsync(_fakeRepositoryApprenticeship);

            var providerId = _fakeRepositoryApprenticeship.ProviderId++;

            Func<Task> act = async () => await _handler.Handle(new GetApprenticeshipRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = providerId
                },
                ApprenticeshipId = _fakeRepositoryApprenticeship.Id
            });

            act.ShouldThrow<UnauthorizedException>().WithMessage($"Provider {providerId} not authorised to access apprenticeship {_fakeRepositoryApprenticeship.Id}, expected provider {_fakeRepositoryApprenticeship.ProviderId}");
        }

        [Test]
        public async Task ThenGetChangeOfPartyResponseIsCalled()
        {
            _mockApprenticeshipRespository.Setup(x => x.GetApprenticeship(It.IsAny<long>())).ReturnsAsync(_fakeRepositoryApprenticeship);

            await _handler.Handle(_exampleValidRequest);

            _mockApprenticeshipRespository.Verify(x => x.GetChangeOfPartyResponse(It.IsAny<long>()), Times.Once);
        }
    }
}
