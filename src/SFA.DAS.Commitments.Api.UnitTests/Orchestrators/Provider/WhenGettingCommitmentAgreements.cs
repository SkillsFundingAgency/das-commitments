using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Application.Queries.GetCommitmentAgreements;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Provider
{
    [TestFixture]
    public class WhenGettingCommitmentAgreements : ProviderOrchestratorTestBase
    {
        [Test]
        public async Task ThenResultIsMappedCommitmentAgreementsReturnedFromGetCommitmentAgreementsQueryHandler()
        {
            const long providerId = 321L;

            MockMediator.Setup(m => m.SendAsync(It.IsAny<GetCommitmentAgreementsRequest>()))
                .ReturnsAsync(new GetCommitmentAgreementsResponse
                {
                    Data = new List<CommitmentAgreement>
                    {
                        new CommitmentAgreement
                        {
                            Reference = "ref",
                            LegalEntityName = "len",
                            AccountLegalEntityPublicHashedId = "aleHash"
                        }
                    }
                });

            var mappedCommitmentAgreement = new Types.Commitment.CommitmentAgreement
            {
                Reference = "mapped ref",
                LegalEntityName = "mapped len",
                AccountLegalEntityPublicHashedId = "mapped aleHash"
            };

            var mockCommitmentMapper = new Mock<ICommitmentMapper>();

            mockCommitmentMapper.Setup(m => m.Map(It.IsAny<Domain.Entities.CommitmentAgreement>()))
                .Returns(TestHelper.Clone(mappedCommitmentAgreement));

            Orchestrator = new ProviderOrchestrator(
                MockMediator.Object,
                Mock.Of<ICommitmentsLogger>(),
                MockFacetMapper.Object,
                MockApprenticeshipFilter.Object,
                new ApprenticeshipMapper(),
                mockCommitmentMapper.Object);

            var result = await Orchestrator.GetCommitmentAgreements(providerId);

            Assert.IsTrue(TestHelper.EnumerablesAreEqual(new[] { mappedCommitmentAgreement }, result));
        }
    }
}
