using System.Collections.Generic;
using AutoFixture.NUnit3;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Mappers
{
    [TestFixture]
    public class WhenMappingACommitmentListItem
    {
        private ICommitmentRules _rules;
        private CommitmentMapper _mapper;

        [SetUp]
        public void Setup()
        {
            _rules = Mock.Of<ICommitmentRules>();
            _mapper = new CommitmentMapper(_rules);
        }

        [Test, AutoData]
        public void ThenBasicMappingIsCorrect(CommitmentSummary commitmentSummary)
        {
            var untouched = Clone(commitmentSummary);

            var commitmentListItem = _mapper.MapFrom(commitmentSummary, CallerType.Employer);

            Assert.AreEqual(untouched.Id, commitmentListItem.Id);
            Assert.AreEqual(untouched.Reference, commitmentListItem.Reference);
            Assert.AreEqual(untouched.ProviderId, commitmentListItem.ProviderId);
            Assert.AreEqual(untouched.ProviderName, commitmentListItem.ProviderName);
            Assert.AreEqual(untouched.EmployerAccountId, commitmentListItem.EmployerAccountId);
            Assert.AreEqual(untouched.LegalEntityId, commitmentListItem.LegalEntityId);
            Assert.AreEqual(untouched.LegalEntityName, commitmentListItem.LegalEntityName);
            Assert.AreEqual((Types.Commitment.Types.CommitmentStatus)untouched.CommitmentStatus, commitmentListItem.CommitmentStatus);
            Assert.AreEqual((Types.Commitment.Types.EditStatus)untouched.EditStatus, commitmentListItem.EditStatus);
            Assert.AreEqual(untouched.ApprenticeshipCount, commitmentListItem.ApprenticeshipCount);
            Assert.AreEqual((Types.AgreementStatus)untouched.AgreementStatus, commitmentListItem.AgreementStatus);
            Assert.AreEqual((Types.Commitment.Types.LastAction)untouched.LastAction, commitmentListItem.LastAction);
            Assert.AreEqual(untouched.TransferSenderId, commitmentListItem.TransferSenderId);
            Assert.AreEqual((Types.TransferApprovalStatus)untouched.TransferApprovalStatus, commitmentListItem.TransferApprovalStatus);
            Assert.AreEqual(untouched.TransferSenderName, commitmentListItem.TransferSenderName);

            Assert.IsNotNull(commitmentListItem.EmployerLastUpdateInfo);
            Assert.AreEqual(untouched.LastUpdatedByEmployerName, commitmentListItem.EmployerLastUpdateInfo.Name);
            Assert.AreEqual(untouched.LastUpdatedByEmployerEmail, commitmentListItem.EmployerLastUpdateInfo.EmailAddress);

            Assert.IsNotNull(commitmentListItem.ProviderLastUpdateInfo);
            Assert.AreEqual(untouched.LastUpdatedByProviderName, commitmentListItem.ProviderLastUpdateInfo.Name);
            Assert.AreEqual(untouched.LastUpdatedByProviderEmail, commitmentListItem.ProviderLastUpdateInfo.EmailAddress);

            Assert.IsNotNull(commitmentListItem.Messages);
            Assert.AreEqual(untouched.Messages.Count, commitmentListItem.Messages.Count);
        }

        [TestCase(true, CallerType.Employer, true, false)]
        [TestCase(true, CallerType.Provider, false, true)]
        public void ThenCanBeApprovedIsMappedAccordingToCallerType(bool expectedCanBeApproved,
            CallerType callerType, bool employerCanApprove, bool providerCanApprove)
        {
            var commitmentSummary = new CommitmentSummary
            {
                EmployerCanApproveCommitment = employerCanApprove,
                ProviderCanApproveCommitment = providerCanApprove,
                Messages = new List<Message>()
            };

            var commitmentListItem = _mapper.MapFrom(commitmentSummary, callerType);

            Assert.AreEqual(expectedCanBeApproved, commitmentListItem.CanBeApproved);
        }

        public static T Clone<T>(T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}