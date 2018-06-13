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

            Assert.AreEqual(_apprenticeships.Count, result.Count);
            Assert.AreEqual(_apprenticeships[0].Id, result[0].Id);
            Assert.AreEqual(_apprenticeships[0].Uln, result[0].ULN);
            Assert.AreEqual(_apprenticeships[0].FirstName, result[0].FirstName);
            Assert.AreEqual(_apprenticeships[0].LastName, result[0].LastName);
            Assert.AreEqual(_apprenticeships[0].DateOfBirth, result[0].DateOfBirth);
            Assert.AreEqual(_apprenticeships[0].TrainingCode, result[0].TrainingCode);
            Assert.AreEqual(_apprenticeships[0].TrainingName, result[0].TrainingName);
            Assert.AreEqual(_apprenticeships[0].Cost, result[0].Cost);
            Assert.AreEqual(_apprenticeships[0].StartDate, result[0].StartDate);
            Assert.AreEqual(_apprenticeships[0].EndDate, result[0].EndDate);
            Assert.AreEqual(_apprenticeships[0].StopDate, result[0].StopDate);
            Assert.AreEqual(_apprenticeships[0].ProviderRef, result[0].ProviderRef);
            Assert.AreEqual(_apprenticeships[0].EmployerRef, result[0].EmployerRef);
            Assert.AreEqual(_apprenticeships[0].EmployerAccountId, result[0].EmployerAccountId);
            Assert.AreEqual(_apprenticeships[0].LegalEntityName, result[0].LegalEntityName);
            Assert.AreEqual(_apprenticeships[0].ProviderId, result[0].ProviderId);
            Assert.AreEqual(_apprenticeships[0].ProviderName, result[0].ProviderName);
            Assert.AreEqual(_apprenticeships[0].TransferSenderId, result[0].TransferSenderId);
            Assert.AreEqual(_apprenticeships[0].CommitmentId, result[0].CommitmentId);

        }

    }
}
