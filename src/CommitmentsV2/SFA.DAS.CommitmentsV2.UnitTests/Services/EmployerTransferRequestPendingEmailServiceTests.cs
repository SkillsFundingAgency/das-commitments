using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.Encoding;
using System.Collections;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    public class EmployerTransferRequestPendingEmailServiceTests
    {
        private EmployerTransferRequestPendingEmailServiceTestsFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new EmployerTransferRequestPendingEmailServiceTestsFixture();
        }

        [TestCaseSource(typeof(DataCases))]
        public async Task ThenSendEmailToEmployerCommandSentForEachEmployerNotification(DataCases.Input input, List<DataCases.Output> outputs)
        {
            // Arrange
            _fixture
                .WithEmployerTransferRequestPendingNotifications(input.EmployerTransferRequestPendingNotifications);

            // Act
            await _fixture.SendEmployerTransferRequestPendingNotifications();

            // Assert
            _fixture.VerifySendEmailToEmployerCommandSent(outputs);
        }

        public class DataCases : IEnumerable
        {            
            public IEnumerator GetEnumerator()
            {
                #region no notifications
                yield return new object[]
                {
                    new Input
                    {
                        EmployerTransferRequestPendingNotifications = new List<EmployerTransferRequestPendingNotification>
                        {
                        }
                    },
                    new List<Output>
                    {
                    }
                };
                #endregion

                #region multiple pending transfer requests
                yield return new object[]
                {
                    new Input
                    {
                        EmployerTransferRequestPendingNotifications = new List<EmployerTransferRequestPendingNotification>
                        {
                            new EmployerTransferRequestPendingNotification
                            {
                                SendingEmployerAccountId = 1001,
                                CohortReference = "AAA",
                                ReceivingLegalEntityName = "FIRST LEGAL ENTITY"
                            },
                            new EmployerTransferRequestPendingNotification
                            {
                                SendingEmployerAccountId = 1002,
                                CohortReference = "BBB",
                                ReceivingLegalEntityName = "SECOND LEGAL ENTITY"
                            },
                            new EmployerTransferRequestPendingNotification
                            {
                                SendingEmployerAccountId = 1003,
                                CohortReference = "CCC",
                                ReceivingLegalEntityName = "THIRD LEGAL ENTITY"
                            }
                        }
                    },
                    new List<Output>
                    {
                        new Output
                        {
                            AccountId = 1001,
                            HashedAccountId = "HSH1001",
                            Tokens = new Dictionary<string, string>
                            {
                                {"cohort_reference", "AAA"},
                                {"receiver_name", "FIRST LEGAL ENTITY"},
                                {"transfers_dashboard_url", $"accounts/HSH1001/transfers"}
                            }
                        },
                        new Output
                        {
                            AccountId = 1002,
                            HashedAccountId = "HSH1002",
                            Tokens = new Dictionary<string, string>
                            {
                                {"cohort_reference", "BBB"},
                                {"receiver_name", "SECOND LEGAL ENTITY"},
                                {"transfers_dashboard_url", $"accounts/HSH1002/transfers"}
                            }
                        },
                        new Output
                        {
                            AccountId = 1003,
                            HashedAccountId = "HSH1003",
                            Tokens = new Dictionary<string, string>
                            {
                                {"cohort_reference", "CCC"},
                                {"receiver_name", "THIRD LEGAL ENTITY"},
                                {"transfers_dashboard_url", $"accounts/HSH1003/transfers"}
                            }
                        }
                    }
                };
                #endregion
            }

            #region Test Data Classes
            public class Input
            {
                public List<EmployerTransferRequestPendingNotification> EmployerTransferRequestPendingNotifications { get; internal set; }
            }

            public class Output
            {
                public int AccountId { get; internal set; }
                public string HashedAccountId { get; internal set; }
                public Dictionary<string, string> Tokens { get; internal set; }
            }

            #endregion
        }

        public class EmployerTransferRequestPendingEmailServiceTestsFixture
        {
            private Mock<ITransferRequestDomainService> _transferRequestDomainService;
            private Mock<IEncodingService> _encodingService;
            private Mock<IMessageSession> _messageSession;

            public EmployerTransferRequestPendingEmailServiceTestsFixture()
            {
                _transferRequestDomainService = new Mock<ITransferRequestDomainService>();
                _encodingService = new Mock<IEncodingService>();
                _encodingService.Setup(m => m.Encode(It.IsAny<long>(), EncodingType.AccountId)).Returns((long value, EncodingType encodingType) => $"HSH{value}");
                _messageSession = new Mock<IMessageSession>();
            }

            public async Task SendEmployerTransferRequestPendingNotifications()
            {
                var service = new EmployerTransferRequestPendingEmailService(_transferRequestDomainService.Object, _encodingService.Object, _messageSession.Object, Mock.Of<ILogger<EmployerTransferRequestPendingEmailService>>());
                await service.SendEmployerTransferRequestPendingNotifications();
            }

            public EmployerTransferRequestPendingEmailServiceTestsFixture WithEmployerTransferRequestPendingNotifications(List<EmployerTransferRequestPendingNotification> employerTransferRequestPendingNotifications)
            {
                _transferRequestDomainService.Setup(m => m.GetEmployerTransferRequestPendingNotifications()).ReturnsAsync(employerTransferRequestPendingNotifications);
                return this;
            }

            public void VerifySendEmailToEmployerCommandSent(List<DataCases.Output> outputs)
            {
                if (outputs.Any())
                {
                    foreach (var output in outputs)
                    {
                        _messageSession.Verify(m => m.Send(It.Is<SendEmailToEmployerCommand>(p =>
                            p.AccountId == output.AccountId &&
                            p.Template == "SendingEmployerTransferRequestNotification" &&
                            p.Tokens.SequenceEqual(output.Tokens)), It.IsAny<SendOptions>()), Times.Once);
                    }
                }
                else
                {
                    _messageSession.Verify(m => m.Send(It.IsAny<SendEmailToEmployerCommand>(), It.IsAny<SendOptions>()), Times.Never);
                }
            }
        }
    }
}
