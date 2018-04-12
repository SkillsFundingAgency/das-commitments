using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.HashingService;
using TransferApprovalStatus = SFA.DAS.Commitments.Api.Types.TransferApprovalStatus;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Mappers
{
    [TestFixture]
    public class WhenMappingATransferRequest
    {
        private TransferRequestMapper _mapper;
        private Domain.Entities.TransferRequest _source;
        private List<Domain.Entities.TrainingCourseSummary> _courses;
        private Mock<IHashingService> _hashingService;

        [SetUp]
        public void Setup()
        {
            _hashingService = new Mock<IHashingService>();
            var fixture = new Fixture();
            _courses = fixture.Create<List<Domain.Entities.TrainingCourseSummary>>();
            _source = fixture.Create<Domain.Entities.TransferRequest>();
            _source.TrainingCourses = JsonConvert.SerializeObject(_courses);
            
            _mapper = new TransferRequestMapper(_hashingService.Object);
        }

        [Test]
        public void ThenMappingTheSourceObjectReturnsTheApiObjectValuesCorrectly()
        {
            var result = _mapper.MapFrom(_source);

            result.TransferRequestId.Should().Be(_source.TransferRequestId);
            result.ApprovedOrRejectedByUserEmail.Should().Be(_source.ApprovedOrRejectedByUserEmail);
            result.ApprovedOrRejectedByUserName.Should().Be(_source.ApprovedOrRejectedByUserName);
            result.ApprovedOrRejectedOn.Should().Be(_source.ApprovedOrRejectedOn);
            result.CommitmentId.Should().Be(_source.CommitmentId);
            result.LegalEntityName.Should().Be(_source.LegalEntityName);
            result.ReceivingEmployerAccountId.Should().Be(_source.ReceivingEmployerAccountId);
            result.SendingEmployerAccountId.Should().Be(_source.SendingEmployerAccountId);
            result.Status.Should().Be((TransferApprovalStatus)_source.Status);
            result.TransferCost.Should().Be(_source.TransferCost);
            result.TrainingList.Count.Should().Be(_courses.Count);
            result.TrainingList[0].ApprenticeshipCount.Should().Be(_courses[0].ApprenticeshipCount);
            result.TrainingList[0].CourseTitle.Should().Be(_courses[0].CourseTitle);

        }

        [Test]
        public void ThenMappingANullSourceObjectReturnsANullResponse()
        {
            var result = _mapper.MapFrom((Domain.Entities.TransferRequest)null);
            result.Should().BeNull();
        }
    }
}
