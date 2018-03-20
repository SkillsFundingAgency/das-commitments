using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Mappers
{
    [TestFixture]
    public class WhenMappingACommitmentView
    {
        private ICommitmentRules _rules;
        private CommitmentMapper _mapper;

        [SetUp]
        public void Arrange()
        {
            _rules = Mock.Of<ICommitmentRules>();
            _mapper = new CommitmentMapper(_rules);
        }

        [TestCase(CallerType.Employer)]
        [TestCase(CallerType.Provider)]
        public void ThenTransferSenderFieldsAreMappedCorrectly(CallerType callerType)
        {
            var commitment = new Commitment
            {
                TransferSenderId = 1,
                TransferSenderName = "Transfer Sender Org"
            };

            var result = _mapper.MapFrom(commitment, callerType);

            Assert.AreEqual(commitment.TransferSenderId, result.TransferSenderId);
            Assert.AreEqual(commitment.TransferSenderName, result.TransferSenderName);
        }
    }
}
