using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

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
            var apprenticeship = new Domain.Entities.Apprenticeship
            {
                Cost = null,
                AgreedOn = null,
                DateOfBirth = null,
                EndDate = null,
                EmployerRef = null,
                FirstName = null,
                PauseDate = null,
                LastName = null,
                LegalEntityId = null,
                LegalEntityName = null,
                NINumber = null,
                ProviderName = null,
                ProviderRef = null,
                Reference = null,
                TrainingCode = null,
                TrainingName = null,
                StartDate = null,
                ULN = null,
                UpdateOriginator = null
            };
            var commitment = new Commitment {Apprenticeships = new List<Domain.Entities.Apprenticeship> {apprenticeship} };

            Assert.DoesNotThrow(() => _mapper.MapFrom(commitment, CallerType.Provider));
        }

        [Test]
        public void ThenMappingThrowsIfCommitmentNull()
        {
            Assert.IsNull(_mapper.MapFrom(null as Commitment, CallerType.Employer));
        }
    }
}
