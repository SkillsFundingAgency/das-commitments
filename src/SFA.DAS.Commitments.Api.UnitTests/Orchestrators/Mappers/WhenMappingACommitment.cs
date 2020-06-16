using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;
using OrganisationType = SFA.DAS.Common.Domain.Types.OrganisationType;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Mappers
{
    [TestFixture]
    public class WhenMappingACommitment
    {
        private ICommitmentRules _rules;

        private CommitmentMapper _mapper;

        [SetUp]
        public void Setup()
        {
            _rules = Mock.Of<ICommitmentRules>();
            _mapper = new CommitmentMapper(_rules);
        }

        [Test]
        public void ThenMappingCompletesSuccessfully()
        {
            var commitment = new Commitment {Apprenticeships = new List<Domain.Entities.Apprenticeship> { new Domain.Entities.Apprenticeship() } };

            Assert.DoesNotThrow(() => _mapper.MapFrom(commitment, CallerType.Provider));
        }

        [Test]
        public void ThenMappingThrowsIfCommitmentNull()
        {
            Assert.IsNull(_mapper.MapFrom(null as Commitment, CallerType.Employer));
        }

        [Test]
        public void FromCommitmentToCommitmentViewThenCommitmentViewIsCorrectlyMapped()
        {
            const string accountLegalEntityPublicHashedId = "123456";

            var from = new Commitment
            {
                Reference = "Reference",
                EmployerAccountId = 1,
                TransferSenderId = 2,
                TransferSenderName = "TransferSenderName",
                LegalEntityId = "LegalEntityId",
                LegalEntityName = "LegalEntityName",
                LegalEntityAddress = "LegalEntityAddress",
                LegalEntityOrganisationType = OrganisationType.Charities,
                ProviderId = 3,
                ProviderName = "ProviderName",
                CommitmentStatus = CommitmentStatus.Active,
                EditStatus = EditStatus.Both,
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId,
            };

            var result = _mapper.MapFrom(from, CallerType.Provider);

            Assert.AreEqual("Reference", result.Reference);
            Assert.AreEqual(1, result.EmployerAccountId);
            Assert.AreEqual(2, result.TransferSender.Id);
            Assert.AreEqual("TransferSenderName", result.TransferSender.Name);
            Assert.AreEqual("LegalEntityId", result.LegalEntityId);
            Assert.AreEqual("LegalEntityName", result.LegalEntityName);
            Assert.AreEqual("LegalEntityAddress", result.LegalEntityAddress);
            Assert.AreEqual(3, result.ProviderId);
            Assert.AreEqual("ProviderName", result.ProviderName);
            Assert.AreEqual(Types.Commitment.Types.EditStatus.Both, result.EditStatus);
            Assert.AreEqual(accountLegalEntityPublicHashedId, result.AccountLegalEntityPublicHashedId);
        }

        [Test]
        public void ThenCommitmentIsCorrectlyMapped()
        {
            const string accountLegalEntityPublicHashedId = "123456";

            var from = new Types.Commitment.Commitment
            {
                Reference = "Reference",
                EmployerAccountId = 1,
                TransferSenderId = 2,
                TransferSenderName = "TransferSenderName",
                LegalEntityId = "LegalEntityId",
                LegalEntityName = "LegalEntityName",
                LegalEntityAddress = "LegalEntityAddress",
                LegalEntityOrganisationType = OrganisationType.Charities,
                ProviderId = 3,
                ProviderName = "ProviderName",
                CommitmentStatus = Types.Commitment.Types.CommitmentStatus.Active,
                EditStatus = Types.Commitment.Types.EditStatus.Both,
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId
            };
            from.EmployerLastUpdateInfo.Name = "Test";
            from.EmployerLastUpdateInfo.EmailAddress = "test@test.com";

            var result = _mapper.MapFrom(from);

            Assert.AreEqual("Reference", result.Reference);
            Assert.AreEqual(1,result.EmployerAccountId);
            Assert.AreEqual(2,result.TransferSenderId);
            Assert.AreEqual("TransferSenderName", result.TransferSenderName);
            Assert.AreEqual("LegalEntityId", result.LegalEntityId);
            Assert.AreEqual("LegalEntityName", result.LegalEntityName);
            Assert.AreEqual("LegalEntityAddress", result.LegalEntityAddress);
            Assert.AreEqual(OrganisationType.Charities, result.LegalEntityOrganisationType);
            Assert.AreEqual(3,result.ProviderId);
            Assert.AreEqual("ProviderName", result.ProviderName);
            Assert.AreEqual(CommitmentStatus.Active, result.CommitmentStatus);
            Assert.AreEqual(EditStatus.Both, result.EditStatus);
            Assert.AreEqual(accountLegalEntityPublicHashedId, result.AccountLegalEntityPublicHashedId);
        }

        [TestCase(CallerType.Employer, true, false)]
        [TestCase(CallerType.Provider, false, true)]
        [TestCase(CallerType.TransferSender, false, false)]
        public void ThenMappingToCanApproveFromCallTypeSetsModeCorrectly(CallerType callerType, bool employerCanApprove, bool providerCanApprove)
        {
            var from = new Commitment
            {
                EmployerCanApproveCommitment = employerCanApprove,
                ProviderCanApproveCommitment = providerCanApprove,
                TransferApprovalStatus = TransferApprovalStatus.Pending
            };

            var result = _mapper.MapFrom(from, callerType);

            result.CanBeApproved.Should().BeTrue();
        }

        [TestCase(CallerType.Employer, true, false)]
        [TestCase(CallerType.Provider, false, true)]
        [TestCase(CallerType.TransferSender, false, false)]
        public void ThenMappingToCanApproveFromCallTypeSetsModeCorrectlyforEachApprenticeship(CallerType callerType, bool employerCanApprove, bool providerCanApprove)
        {
            var from = new Commitment();
            from.Apprenticeships.Add(new Domain.Entities.Apprenticeship
            {
                EmployerCanApproveApprenticeship = employerCanApprove,
                ProviderCanApproveApprenticeship = providerCanApprove
            });

            var result = _mapper.MapFrom(from, callerType);

            result.Apprenticeships[0].CanBeApproved.Should().BeTrue();
        }

        [Test]
        public void ThenApprenticeOriginalStartDateIsMapped()
        {
            var fixture = new Fixture();
            var originalStartDate = fixture.Create<DateTime?>();
            var commitment = new Commitment { Apprenticeships = new List<Domain.Entities.Apprenticeship> { new Domain.Entities.Apprenticeship { OriginalStartDate = originalStartDate } } };
            var result = _mapper.MapFrom(commitment, CallerType.Provider);

            Assert.AreEqual(originalStartDate, result.Apprenticeships[0].OriginalStartDate);
        }
    }
}
