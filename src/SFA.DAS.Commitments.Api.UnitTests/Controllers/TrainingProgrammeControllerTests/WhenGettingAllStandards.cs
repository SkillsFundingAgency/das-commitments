using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Results;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Types.TrainingProgramme;
using SFA.DAS.Commitments.Application.Queries.GetAllStandardTrainingProgrammes;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.TrainingProgrammeControllerTests
{
    public class WhenGettingAllStandards
    {
        private Mock<IMediator> _mediator;
        private TrainingProgrammeController _controller;
        private GetAllStandardTrainingProgrammesQueryResponse _queryResult;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture();
            _queryResult = fixture.Create<GetAllStandardTrainingProgrammesQueryResponse>();
            _mediator = new Mock<IMediator>();
            
            _controller = new TrainingProgrammeController(_mediator.Object, Mock.Of<ICommitmentsLogger>());
        }
        
        [Test]
        public async Task Then_The_Request_Is_Passed_To_Mediator_And_Data_Returned()
        {
            _mediator.Setup(x => x.SendAsync(It.IsAny<GetAllStandardTrainingProgrammesQuery>()))
                .ReturnsAsync(_queryResult);
            
            var actual = await _controller.GetAllStandards() as OkNegotiatedContentResult<GetAllTrainingProgrammeStandardsResponse>;;
            
            //actual
            Assert.IsNotNull(actual);
            actual.Content.TrainingProgrammes.Should().BeEquivalentTo(_queryResult.TrainingProgrammes);
        }
        
        [Test]
        public async Task Then_If_There_Is_An_Error_A_Bad_Request_Is_Returned()
        {
            _mediator.Setup(x => x.SendAsync(It.IsAny<GetAllStandardTrainingProgrammesQuery>()))
                .Throws<InvalidOperationException>();
            
            var controllerResult = await _controller.GetAllStandards() as BadRequestResult;

            Assert.IsNotNull(controllerResult);
        }
    }
}