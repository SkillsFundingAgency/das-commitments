using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountLegalEntity;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
    [TestFixture]
    public class EmployerAgreementControllerTests
    {
        [Test]
        public async Task IsAgreementSignedForFeature_WithValidModelAndExistingLegalEntity_ShouldReturnOkayAndContent()
        {
            var f = new EmployerAgreementControllerTestFixtures();

            var response = await f.Sut.IsAgreementSignedForFeature(f.AgreementSignedRequest);

            Assert.AreEqual(typeof(OkObjectResult), response.GetType());
            var objectResult = (OkObjectResult) response;
            Assert.AreEqual(200, objectResult.StatusCode);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task IsAgreementSignedForFeature_WithValidModelAndExistingLegalEntity_ShouldReturnExpectedBoolean(bool expected)
        {
            var f = new EmployerAgreementControllerTestFixtures().WithAgreementIdSignedAs(expected);

            var response = await f.Sut.IsAgreementSignedForFeature(f.AgreementSignedRequest);

            Assert.IsInstanceOf<OkObjectResult>(response);
            Assert.AreEqual(expected, (bool)((OkObjectResult)response).Value);
        }

        //[Test]
        //public async Task GetAccountLegalEntity_WithValidModelAndExistingId_ShouldResultMappedCorrectly()
        //{
        //    const long accountLegalEntityId = 456;

        //    // arrange
        //    var fixtures = new AccountLegalEntityControllerTestFixtures()
        //        .SetQueryResponse(accountLegalEntityId, new GetAccountLegalEntityResponse { AccountId = 1, MaLegalEntityId = 234, AccountName = "AccountName", LegalEntityName = "ABC" });

        //    // act
        //    var response = await fixtures.CallControllerMethod(accountLegalEntityId);

        //    // Assert
        //    var model = response
        //        .VerifyReturnsModel()
        //        .WithModel<AccountLegalEntityResponse>();

        //    Assert.AreEqual(1, model.AccountId);
        //    Assert.AreEqual(234, model.MaLegalEntityId);
        //    Assert.AreEqual("AccountName", model.AccountName);
        //    Assert.AreEqual("ABC", model.LegalEntityName);
        //}

        //[Test]
        //public async Task GetAccountLegalEntity_WithValidModelButInvalidId_ShouldReturnNotFound()
        //{
        //    const long accountLegalEntityId = 456;

        //    // arrange
        //    var fixtures = new AccountLegalEntityControllerTestFixtures()
        //        .SetQueryResponse(accountLegalEntityId, null);

        //    // act
        //    var response = await fixtures.CallControllerMethod(accountLegalEntityId);

        //    // Assert
        //    Assert.AreEqual(typeof(NotFoundResult), response.GetType());

        //    var objectResult = (NotFoundResult) response;

        //    Assert.AreEqual(404, objectResult.StatusCode);
        //}
    }

        public class EmployerAgreementControllerTestFixtures
    {
        public EmployerAgreementControllerTestFixtures()
        {
            var autoFixture = new Fixture();
            AccountLegalEntityId = autoFixture.Create<long>();
            AccountLegalEntity = autoFixture.Create<GetAccountLegalEntityResponse>();
            AgreementSignedRequest = autoFixture.Create<AgreementSignedRequest>();

            MediatorMock = new Mock<IMediator>();
            MediatorMock.Setup(m => m.Send(It.IsAny<GetAccountLegalEntityRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(AccountLegalEntity));

            EmployerAgreementService = new Mock<IEmployerAgreementService>();

            Sut = new EmployerAgreementController(MediatorMock.Object, EmployerAgreementService.Object);
        }

        public long AccountLegalEntityId;
        public AgreementSignedRequest AgreementSignedRequest;
        public GetAccountLegalEntityResponse AccountLegalEntity;
        public Mock<IMediator> MediatorMock { get; set; }
        public Mock<IEmployerAgreementService> EmployerAgreementService { get; set; }
        public EmployerAgreementController Sut { get; }

        public EmployerAgreementControllerTestFixtures WithAgreementIdSignedAs(bool isSigned)
        {
            EmployerAgreementService.Setup(x => x.IsAgreementSigned(It.IsAny<long>(), It.IsAny<long>(), 
                    It.IsAny<AgreementFeature[]>()))
                .ReturnsAsync(isSigned);

            return this;
        }

    }
}