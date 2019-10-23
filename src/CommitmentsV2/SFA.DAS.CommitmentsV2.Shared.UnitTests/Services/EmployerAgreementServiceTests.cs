using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Shared.Models;
using SFA.DAS.CommitmentsV2.Shared.Services;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.Services
{
    [TestFixture]
    [Parallelizable]
    public class EmployerAgreementServiceTests
    {

        [Test]
        public async Task IsAgreementSigned_ItShouldCallAccountApiEndpointWithCorrectParameters()
        {
            var f = new EmployerAgreementServiceTestsFixture();
            await f.Sut.IsAgreementSigned(f.AccountId, f.AccountLegalEntityId);
            f.VerifyAccountApiClientReceivesCorrectValues();
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public async Task IsAgreementSigned_WithoutAnyAgreementFeatureAndWithOneSignedAgreementAtSpecifiedVersion_ItShouldReturnTrue(int version)
        {
            var f = new EmployerAgreementServiceTestsFixture().SetUpSignedAgreementWithVersion(version);
            var result = await f.Sut.IsAgreementSigned(f.AccountId, f.AccountLegalEntityId);
            Assert.IsTrue(result);
        }

        [TestCase(1, false)]
        [TestCase(2, true)]
        [TestCase(3, true)]
        public async Task IsAgreementSigned_WithTransferAgreementFeatureAndWithOneSignedAgreementAtSpecifiedVersion_ItShouldReturnExpectedValue(int version, bool expected)
        {
            var f = new EmployerAgreementServiceTestsFixture().SetUpSignedAgreementWithVersion(version);
            var result = await f.Sut.IsAgreementSigned(f.AccountId, f.AccountLegalEntityId, AgreementFeature.Transfers);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public async Task IsAgreementSigned_WithTransferAgreementFeatureAndWithMultipleSignedAgreements_ItShouldReturnTrue()
        {
            var f = new EmployerAgreementServiceTestsFixture().SetUpSignedAgreementWithVersion(1).SetUpSignedAgreementWithVersion(2);
            var result = await f.Sut.IsAgreementSigned(f.AccountId, f.AccountLegalEntityId, AgreementFeature.Transfers);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task IsAgreementSigned_WithoutAnyAgreementFeaturesAndWithMultipleSignedAgreements_ItShouldReturnTrue()
        {
            var f = new EmployerAgreementServiceTestsFixture().SetUpSignedAgreementWithVersion(1).SetUpSignedAgreementWithVersion(2);
            var result = await f.Sut.IsAgreementSigned(f.AccountId, f.AccountLegalEntityId);
            Assert.IsTrue(result);
        }
    }

    public class EmployerAgreementServiceTestsFixture
    {
        public long AccountId = 123;
        public long AccountLegalEntityId = 456;
        public LegalEntityViewModel LegalEntityViewModel;
        public Mock<IAccountApiClient> AccountApiClient;
        public Mock<IEncodingService> EncodingService;
        public EmployerAgreementService Sut;

        public EmployerAgreementServiceTestsFixture()
        {
            LegalEntityViewModel = new LegalEntityViewModel();
            LegalEntityViewModel.Agreements = new List<AgreementViewModel>();

            EncodingService = new Mock<IEncodingService>();
            EncodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.AccountId)).Returns((long x, EncodingType t) => $"X{x}X");

            AccountApiClient = new Mock<IAccountApiClient>();
            AccountApiClient.Setup(x => x.GetLegalEntity(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(LegalEntityViewModel);

            Sut = new EmployerAgreementService(AccountApiClient.Object, EncodingService.Object, Mock.Of<ILogger<EmployerAgreementService>>());
        }

        public EmployerAgreementServiceTestsFixture SetUpSignedAgreementWithVersion(int version)
        {
            LegalEntityViewModel.Agreements.Add(new AgreementViewModel { Status = EmployerAgreementStatus.Signed, TemplateVersionNumber = version });
            return this;
        }

        public void VerifyAccountApiClientReceivesCorrectValues()
        {
            AccountApiClient.Verify(x => x.GetLegalEntity($"X{AccountId}X", AccountLegalEntityId), Times.Once);
        }
    }
}