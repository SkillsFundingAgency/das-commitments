using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using AutoFixture;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
    [TestFixture]
    public class AuthorizationControllerTests
    {
        private AuthorizationControllerTestFixture _fixture;

        [SetUp]
        public void Initialise()
        {
            _fixture = new AuthorizationControllerTestFixture();
        }

        [Test]
        public async Task AuthorizationController_AccessCohortRequest_ShouldReturnAnOkResultWithFalseWhenNoCohortFound()
        {
            var response = await _fixture.AuthorizationController.CanAccessCohort(_fixture.PartyType, _fixture.PartyId, _fixture.CohortId);

            Assert.IsFalse((bool)((OkObjectResult)response).Value);
        }

        [TestCase(PartyType.Provider)]
        [TestCase(PartyType.Employer)]
        public async Task AuthorizationController_AccessCohortRequest_ShouldReturnAnOkResultWithTrueWhenCohortFoundAndPartyTypeAndPartyIdMatches(PartyType partyType)
        {
            _fixture.SetGetCohortSummaryResponseTo(partyType, _fixture.PartyId);

            var response = await _fixture.AuthorizationController.CanAccessCohort(partyType, _fixture.PartyId, _fixture.CohortId);

            Assert.IsTrue((bool)((OkObjectResult)response).Value);
        }

        [TestCase(PartyType.Provider, "828")]
        [TestCase(PartyType.Provider, "NotANumber")]
        [TestCase(PartyType.Employer, "828")]
        [TestCase(PartyType.Employer, "NotANumber")]
        public async Task AuthorizationController_AccessCohortRequest_ShouldReturnAnOkResultWithFalseWhenCohortFoundAndPartyTypeAndPartyIdDoesNotMatch(PartyType partyType, string nonMatchingPartyId)
        {
            _fixture.SetGetCohortSummaryResponseTo(partyType, _fixture.PartyId);

            var response = await _fixture.AuthorizationController.CanAccessCohort(partyType, nonMatchingPartyId, _fixture.CohortId);

            Assert.IsFalse((bool)((OkObjectResult)response).Value);
        }
    }

    public class AuthorizationControllerTestFixture
    {
        public AuthorizationControllerTestFixture()
        {
            var fixture = new Fixture();
            MediatorMock = new Mock<IMediator>();
            PartyId = "876876";
            PartyType = fixture.Create<PartyType>();
            CohortId = fixture.Create<long>();
            GetCohortSummaryResponse = fixture.Build<GetCohortSummaryResponse>().Create();

            MediatorMock
                .Setup(m => m.Send(It.IsAny<GetCohortSummaryRequest>(), CancellationToken.None))
                .ReturnsAsync(GetCohortSummaryResponse);


            AuthorizationController = new AuthorizationController(MediatorMock.Object);
        }

        public string PartyId { get; set; }
        public PartyType PartyType { get; set; }
        public long CohortId { get; set; }
        public GetCohortSummaryResponse GetCohortSummaryResponse { get; set; }
        public Mock<IMediator> MediatorMock { get; }

        public AuthorizationController AuthorizationController { get; }

        public AuthorizationControllerTestFixture SetGetCohortAccessRequestTo(PartyType partyType, string partyId)
        {
            PartyType = partyType;
            PartyId = partyId;

            return this;
        }

        public AuthorizationControllerTestFixture SetGetCohortSummaryResponseTo(PartyType partyType, string partyId)
        {
            switch (partyType)
            {
                case PartyType.Provider:
                    GetCohortSummaryResponse.ProviderId = long.Parse(partyId);
                    break;
                case PartyType.Employer:
                    GetCohortSummaryResponse.AccountId = long.Parse(partyId);
                    break;
            }

            return this;
        }
    }
}