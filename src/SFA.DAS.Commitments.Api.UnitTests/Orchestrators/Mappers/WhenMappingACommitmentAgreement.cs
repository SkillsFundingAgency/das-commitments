using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Application.Rules;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Mappers
{
    [TestFixture]
    public class WhenMappingACommitmentAgreement
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
            const string reference = "COMREF", legalEntityName = "len", aleHash = "alehash";

            var sourceDomainCommitmentAgreement = new Domain.Entities.CommitmentAgreement
            {
                Reference = reference,
                LegalEntityName = legalEntityName,
                AccountLegalEntityPublicHashedId = aleHash
            };

            var mappedTypesCommitmentAgreement = _mapper.Map(sourceDomainCommitmentAgreement);

            Assert.AreEqual(reference, mappedTypesCommitmentAgreement.Reference);
            Assert.AreEqual(legalEntityName, mappedTypesCommitmentAgreement.LegalEntityName);
            Assert.AreEqual(aleHash, mappedTypesCommitmentAgreement.AccountLegalEntityPublicHashedId);
        }
    }
}
