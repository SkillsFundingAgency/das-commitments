using System;
using System.Threading.Tasks;
using System.Web.Http.Results;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Types.TrainingProgramme;
using SFA.DAS.Commitments.Application.Queries.GetTrainingProgramme;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.TrainingProgrammeControllerTests
{
    public class WhenGettingATrainingProgramme
    {
        private Mock<IMediator> _mediator;
        private TrainingProgrammeController _controller;
        private GetTrainingProgrammeQueryResponse _queryResult;
        private string _trainingCode;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture();
            _trainingCode = fixture.Create<string>();
            _queryResult = fixture.Create<GetTrainingProgrammeQueryResponse>();
            _mediator = new Mock<IMediator>();
            
            _controller = new TrainingProgrammeController(_mediator.Object, Mock.Of<ICommitmentsLogger>());
        }
        
        [Test]
        public async Task Then_The_Request_Is_Passed_To_Mediator_And_Data_Returned()
        {
            
            _mediator.Setup(x => x.SendAsync(It.Is<GetTrainingProgrammeQuery>(c=>c.Id.Equals(_trainingCode))))
                .ReturnsAsync(_queryResult);
            
            var actual = await _controller.GetTrainingProgramme(_trainingCode) as OkNegotiatedContentResult<GetTrainingProgrammeResponse>;;
            
            //actual
            Assert.IsNotNull(actual);
            actual.Content.TrainingProgramme.ShouldBeEquivalentTo(_queryResult.TrainingProgramme);
        }
        
        [Test]
        public async Task Then_If_There_Is_An_Error_A_Bad_Request_Is_Returned()
        {
            _mediator.Setup(x => x.SendAsync(It.Is<GetTrainingProgrammeQuery>(c=>c.Id.Equals(_trainingCode))))
                .Throws<InvalidOperationException>();
            
            var controllerResult = await _controller.GetTrainingProgramme(_trainingCode) as BadRequestResult;

            Assert.IsNotNull(controllerResult);
        }
    }
}