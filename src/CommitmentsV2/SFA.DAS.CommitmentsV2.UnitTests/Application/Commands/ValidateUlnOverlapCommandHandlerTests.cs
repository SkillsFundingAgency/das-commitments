using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateUln;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class ValidateUlnOverlapCommandHandlerTests
    {
        [TestCase(true, true)]
        [TestCase(false, false)]
        public async Task Handle_WhenHasOverlapIsMappedCorrect(bool hasOverlap, bool expectedResult)
        {
            var fixture = new ValidateUlnOverlapCommandHandlerTestsFixture()
                .SetUlnOverlap(hasOverlap);
            
            var result = await fixture.Handle();

            Assert.Multiple(() =>
            {
                Assert.That(result.HasOverlappingEndDate, Is.EqualTo(expectedResult));
                Assert.That(result.HasOverlappingStartDate, Is.EqualTo(expectedResult));
            });
        }
    }

    public class ValidateUlnOverlapCommandHandlerTestsFixture
    {
        public IFixture AutoFixture { get; set; }
        public ValidateUlnOverlapCommand Command { get; set; }
        public IRequestHandler<ValidateUlnOverlapCommand, ValidateUlnOverlapResult> Handler { get; set; }
        public Mock<IOverlapCheckService> OverlapCheckService { get; set; }

        public ValidateUlnOverlapCommandHandlerTestsFixture()
        {
            AutoFixture = new Fixture();
            Command = AutoFixture.Create<ValidateUlnOverlapCommand>();
            OverlapCheckService = new Mock<IOverlapCheckService>();
            
            Handler = new ValidateUlnOverlapCommandHandler(OverlapCheckService.Object);
        }

        public Task<ValidateUlnOverlapResult> Handle()
        {
            return Handler.Handle(Command, CancellationToken.None);
        }

        public ValidateUlnOverlapCommandHandlerTestsFixture SetUlnOverlap(bool hasOverlap)
        {
            OverlapCheckService.Setup(x => x.CheckForOverlaps(It.IsAny<string>(), It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(), It.IsAny<long>(), CancellationToken.None))
                .ReturnsAsync(new CommitmentsV2.Domain.Entities.OverlapCheckResult(hasOverlap, hasOverlap));

            return this;
        }
    }
}