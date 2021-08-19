using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgramme;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetTrainingProgramme
{
    public class WhenGettingATrainingProgramme
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_The_Data_Is_Retrieved_From_The_Service(
            GetTrainingProgrammeQuery query,
            TrainingProgramme result,
            [Frozen] Mock<ITrainingProgrammeLookup> service,
            GetTrainingProgrammeQueryHandler handler)
        {
            service.Setup(x => x.GetTrainingProgramme(query.Id)).ReturnsAsync(result);
            
            var actual = await handler.Handle(query, CancellationToken.None);

            actual.TrainingProgramme.Should().BeEquivalentTo(result, opt => opt.Excluding(x => x.Options));
        }
        
        [Test, RecursiveMoqAutoData]
        public async Task Then_Null_Is_Returned_From_The_Service_When_There_Is_No_Course(
            GetTrainingProgrammeQuery query,
            TrainingProgramme result,
            [Frozen] Mock<ITrainingProgrammeLookup> service,
            GetTrainingProgrammeQueryHandler handler)
        {
            service.Setup(x => x.GetTrainingProgramme(query.Id)).ReturnsAsync((TrainingProgramme) null);
            
            var actual = await handler.Handle(query, CancellationToken.None);

            actual.TrainingProgramme.Should().BeNull();
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_If_An_Exception_Is_Thrown_Then_Null_Is_Returned(
            GetTrainingProgrammeQuery query,
            TrainingProgramme result,
            [Frozen] Mock<ITrainingProgrammeLookup> service,
            GetTrainingProgrammeQueryHandler handler)
        {
            service.Setup(x => x.GetTrainingProgramme(query.Id)).ThrowsAsync(new Exception("Course not found"));
            
            var actual  = await handler.Handle(query, CancellationToken.None);

            actual.TrainingProgramme.Should().BeNull();
        }
    }
}