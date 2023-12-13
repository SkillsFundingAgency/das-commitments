using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateChangeOfEmployerOverlap;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.OverlappingTrainingDate
{
    public class ValidateChangeOfEmployerOverlapCommandHandlerTests
    {
        private ValidateChangeOfEmployerOverlapFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ValidateChangeOfEmployerOverlapFixture();
        }

        [Test]
        public async Task ValidateChangeOfEmployerOverlap_Validated()
        {
            await _fixture.ValidateChangeOfEmployerOverlap();
            _fixture.VerifyCommandSend();
        }

        private class ValidateChangeOfEmployerOverlapFixture
        {
            private readonly ValidateChangeOfEmployerOverlapCommandHandler _handler;
            private readonly Fixture _autoFixture;
            private readonly ValidateChangeOfEmployerOverlapCommand _command;
            private readonly Mock<IChangeOfPartyRequestDomainService> _changeOfPartyRequestDomainService;
            private readonly DateTime _stDate;
            private readonly DateTime _edDate;

            public ValidateChangeOfEmployerOverlapFixture()
            {
                _autoFixture = new Fixture();

                var startDate = _autoFixture.Create<DateTime>();
                var endDate = _autoFixture.Create<DateTime>();

                _command = _autoFixture.Build<ValidateChangeOfEmployerOverlapCommand>()
                  .With(x => x.StartDate, startDate.ToString("dd-MM-yyyy"))
                  .With(x => x.EndDate, endDate.ToString("dd-MM-yyyy"))                  
                  .Create();

                _changeOfPartyRequestDomainService = new Mock<IChangeOfPartyRequestDomainService>();

                _stDate = System.DateTime.ParseExact(_command.StartDate, "dd-MM-yyyy", null);
                _edDate = System.DateTime.ParseExact(_command.EndDate, "dd-MM-yyyy", null);

                _handler = new ValidateChangeOfEmployerOverlapCommandHandler(_changeOfPartyRequestDomainService.Object);
            }

            public async Task ValidateChangeOfEmployerOverlap()
            {
                await _handler.Handle(_command, CancellationToken.None);
            }

            public void VerifyCommandSend()
            {
                _changeOfPartyRequestDomainService.Verify(
                    m => m.ValidateChangeOfEmployerOverlap(_command.Uln,
                        _stDate,
                        _edDate,
                        It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}
