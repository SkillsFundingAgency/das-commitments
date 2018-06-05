using System;
using System.Collections.Generic;
using System.Linq;
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
    public class WhenMappingAnApprenticeshipResult
    {
        private ApprenticeshipMapper _mapper;
        private List<ApprenticeshipResult> _apprenticeships;

        [SetUp]
        public void Setup()
        {
            var fixture = new Fixture();
            _apprenticeships = fixture.Create<List<ApprenticeshipResult>>();
            _mapper = new ApprenticeshipMapper();
        }

        [Test]
        public void ThenMappingListCompletesSuccessfully()
        {
            Assert.DoesNotThrow(() => _mapper.MapFrom(_apprenticeships));
        }

        [Test]
        public void ThenMapsApprenticeshipValuesCorrectly()
        {
            var result = _mapper.MapFrom(_apprenticeships).ToList();

            Assert.AreEqual(result.Count, _apprenticeships.Count);
            Assert.AreEqual(result[0].Id, _apprenticeships[0].Id);
            Assert.AreEqual(result[0].ULN, _apprenticeships[0].Uln);
            Assert.AreEqual(result[0].FirstName, _apprenticeships[0].FirstName);
            Assert.AreEqual(result[0].LastName, _apprenticeships[0].LastName);
            Assert.AreEqual(result[0].DateOfBirth, _apprenticeships[0].DateOfBirth);
            Assert.AreEqual(result[0].TrainingCode, _apprenticeships[0].TrainingCode);
            Assert.AreEqual(result[0].TrainingName, _apprenticeships[0].TrainingName);
            Assert.AreEqual(result[0].Cost, _apprenticeships[0].Cost);
            Assert.AreEqual(result[0].StartDate, _apprenticeships[0].StartDate);
            Assert.AreEqual(result[0].EndDate, _apprenticeships[0].EndDate);
            Assert.AreEqual(result[0].StopDate, _apprenticeships[0].StopDate);
            Assert.AreEqual(result[0].ProviderRef, _apprenticeships[0].ProviderRef);
            Assert.AreEqual(result[0].EmployerRef, _apprenticeships[0].EmployerRef);
            Assert.AreEqual(result[0].EmployerAccountId, _apprenticeships[0].EmployerAccountId);
            Assert.AreEqual(result[0].LegalEntityName, _apprenticeships[0].LegalEntityName);
            Assert.AreEqual(result[0].ProviderId, _apprenticeships[0].ProviderId);
            Assert.AreEqual(result[0].ProviderName, _apprenticeships[0].ProviderName);
            Assert.AreEqual(result[0].TransferSenderId, _apprenticeships[0].TransferSenderId);
            Assert.AreEqual(result[0].CommitmentId, _apprenticeships[0].CommitmentId);

        }

    }
}
