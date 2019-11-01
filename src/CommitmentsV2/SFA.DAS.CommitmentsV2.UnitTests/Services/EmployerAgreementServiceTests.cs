using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    [Parallelizable]
    public class EmployerAgreementServiceTests
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public async Task IsAgreementSigned_WithoutAnyAgreementFeatureAndWithOneSignedAgreementAtSpecifiedVersion_ItShouldReturnTrue(int version)
        {
            var f = new EmployerAgreementServiceTestsFixture().SetUpSignedAgreementWithVersion(version);
            var result = await f.Sut.IsAgreementSigned(f.AccountId, f.MaLegalEntityId);
            Assert.IsTrue(result);
        }

        [TestCase(1, false)]
        [TestCase(2, true)]
        [TestCase(3, true)]
        public async Task IsAgreementSigned_WithTransferAgreementFeatureAndWithOneSignedAgreementAtSpecifiedVersion_ItShouldReturnExpectedValue(int version, bool expected)
        {
            var f = new EmployerAgreementServiceTestsFixture().SetUpSignedAgreementWithVersion(version);
            var result = await f.Sut.IsAgreementSigned(f.AccountId, f.MaLegalEntityId, AgreementFeature.Transfers);
            Assert.AreEqual(expected, result);
            f.VerifyAccountApiClientReceivesCorrectValues();
        }

        [Test]
        public async Task IsAgreementSigned_WithTransferAgreementFeatureAndWithMultipleSignedAgreements_ItShouldReturnTrue()
        {
            var f = new EmployerAgreementServiceTestsFixture().SetUpSignedAgreementWithVersion(1).SetUpSignedAgreementWithVersion(2);
            var result = await f.Sut.IsAgreementSigned(f.AccountId, f.MaLegalEntityId, AgreementFeature.Transfers);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task IsAgreementSigned_WithoutAnyAgreementFeaturesAndWithMultipleSignedAgreements_ItShouldReturnTrue()
        {
            var f = new EmployerAgreementServiceTestsFixture().SetUpSignedAgreementWithVersion(1).SetUpSignedAgreementWithVersion(2);
            var result = await f.Sut.IsAgreementSigned(f.AccountId, f.MaLegalEntityId);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task GetLatestAgreementId_WithNoAgreements_ItShouldReturnNull()
        {
            var f = new EmployerAgreementServiceTestsFixture();
            var result = await f.Sut.GetLatestAgreementId(f.AccountId, f.MaLegalEntityId);
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetLatestAgreementId_WithMultipleSignedAgreements_ItShouldReturnId2()
        {
            var f = new EmployerAgreementServiceTestsFixture().SetUpSignedAgreementWithVersion(1).SetUpSignedAgreementWithVersion(2);
            var result = await f.Sut.GetLatestAgreementId(f.AccountId, f.MaLegalEntityId);
            Assert.AreEqual(2, result);
        }

        [Test]
        public async Task GetLatestAgreementId_WithOneSignedAgreementsAndNewUnsignedAgreement_ItShouldReturn2()
        {
            var f = new EmployerAgreementServiceTestsFixture().SetUpSignedAgreementWithVersion(1).SetUpUnsignedAgreementWithVersion(2);
            var result = await f.Sut.GetLatestAgreementId(f.AccountId, f.MaLegalEntityId);
            Assert.AreEqual(2, result);
        }
    }

    public class EmployerAgreementServiceTestsFixture
    {
        public long AccountId = 123;
        public long AccountLegalEntityId = 456;
        public long MaLegalEntityId = 777;
        public LegalEntityViewModel LegalEntityViewModel;
        public AccountLegalEntityResponse AccountLegalEntityResponse;
        public Mock<IAccountApiClient> AccountApiClient;
        public Mock<IEncodingService> EncodingService;
        public EmployerAgreementService2 Sut;

        public EmployerAgreementServiceTestsFixture()
        {
            LegalEntityViewModel = new LegalEntityViewModel();
            LegalEntityViewModel.Agreements = new List<AgreementViewModel>();

            AccountLegalEntityResponse = new AccountLegalEntityResponse();
            AccountLegalEntityResponse.MaLegalEntityId = MaLegalEntityId;

            EncodingService = new Mock<IEncodingService>();
            EncodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.AccountId)).Returns((long x, EncodingType t) => $"X{x}X");

            AccountApiClient = new Mock<IAccountApiClient>();
            AccountApiClient.Setup(x => x.GetLegalEntity(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(LegalEntityViewModel);


            Sut = new EmployerAgreementService2(AccountApiClient.Object, EncodingService.Object, Mock.Of<ILogger<EmployerAgreementService2>>());
        }

        public EmployerAgreementServiceTestsFixture SetUpSignedAgreementWithVersion(int version)
        {
            LegalEntityViewModel.Agreements.Add(new AgreementViewModel { Id = version, Status = EmployerAgreementStatus.Signed, TemplateVersionNumber = version });
            return this;
        }

        public EmployerAgreementServiceTestsFixture SetUpUnsignedAgreementWithVersion(int version)
        {
            LegalEntityViewModel.Agreements.Add(new AgreementViewModel { Id = version, Status = EmployerAgreementStatus.Signed, TemplateVersionNumber = version });
            return this;
        }

        public void VerifyAccountApiClientReceivesCorrectValues()
        {
            AccountApiClient.Verify(x => x.GetLegalEntity($"X{AccountId}X", MaLegalEntityId), Times.Once);
        }
    }
}