using System;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers.OverlappingTrainingDateRequest
{
    [TestFixture]
    [Parallelizable]
    public class OverlappingTrainingDateRequestRejectedEventHandlerTests
    {
        [Test]
        public async Task Handle_ApprenticeshipStoppedEvent_ThenShouldResolvePendingOverlappingTrainingDateRequest()
        {
            var fixture = new OverlappingTrainingDateRequestRejectedEventHandlerTestFixture();
            await fixture.Handle();
            fixture.Verify_EmailToProviderCommand_Sent();
        }

        private class OverlappingTrainingDateRequestRejectedEventHandlerTestFixture
        {
            private OverlappingTrainingDateRequestRejectedEvent _overlappingTrainingDateRequestRejectedEvent;
            private Mock<IMessageHandlerContext> _messageHandlerContext;
            private OverlappingTrainingDateRequestRejectedEventHandler _sut;
            private Models.OverlappingTrainingDateRequest _overlappingTrainingDateRequest;
            private readonly ProviderCommitmentsDbContext _db;
            private Apprenticeship _apprenticeship;
            private DraftApprenticeship _draftApprenticeship;
            private readonly CommitmentsV2Configuration _configuration;

            public OverlappingTrainingDateRequestRejectedEventHandlerTestFixture()
            {
                _configuration = new CommitmentsV2Configuration()
                {
                    ProviderCommitmentsBaseUrl = "BaseUrl/"
                };

                _overlappingTrainingDateRequestRejectedEvent = new OverlappingTrainingDateRequestRejectedEvent()
                {
                    OverlappingTrainingDateRequestId = 1
                };

                _messageHandlerContext = new Mock<IMessageHandlerContext>();

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options);
                SeedDb();

                _sut = new OverlappingTrainingDateRequestRejectedEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db), Mock.Of<ILogger<OverlappingTrainingDateRequestRejectedEventHandler>>(), _configuration);
            }

            public async Task Handle()
            {
                await _sut.Handle(_overlappingTrainingDateRequestRejectedEvent, _messageHandlerContext.Object);
            }

            public void Verify_EmailToProviderCommand_Sent()
            {
                _messageHandlerContext.Verify(m => m.Send(It.Is<SendEmailToProviderCommand>(c =>
                   c.Template == "OverlappingTrainingDateRequestRejected" &&
                   c.Tokens["CohortReference"] == _draftApprenticeship.Cohort.Reference &&
                   c.Tokens["URL"] == $"{_configuration.ProviderCommitmentsBaseUrl}{_draftApprenticeship.Cohort.ProviderId}/unapproved/{_draftApprenticeship.Cohort.Reference}/details"
                   )
                 , It.IsAny<SendOptions>()), Times.Once);
            }

            private void SeedDb()
            {
                var accountLegalEntity = new AccountLegalEntity();
                accountLegalEntity.SetValue(x => x.Name, "PreviousEmployer");
                accountLegalEntity.SetValue(x => x.Id, 1);

                _apprenticeship = new Apprenticeship();
                _apprenticeship.SetValue(x => x.Id, 1);

                _apprenticeship.SetValue(x => x.FirstName, "FirstName");
                _apprenticeship.SetValue(x => x.LastName, "LastName");
                _apprenticeship.SetValue(x => x.Cohort, new Cohort
                {
                    AccountLegalEntity = accountLegalEntity,
                    ProviderId = 1,
                    Reference = "ABC"
                });

                var accountLegalEntity2 = new AccountLegalEntity();
                accountLegalEntity2.SetValue(x => x.Name, "NewEmployer");
                accountLegalEntity2.SetValue(x => x.Id, 2);

                _draftApprenticeship = new DraftApprenticeship();
                _draftApprenticeship.SetValue(x => x.Id, 2);
                _draftApprenticeship.SetValue(x => x.Cohort, new Cohort
                {
                    AccountLegalEntity = accountLegalEntity2,
                    ProviderId = 2,
                    Reference = "XYZ"
                });

                _overlappingTrainingDateRequest = new Models.OverlappingTrainingDateRequest();
                _overlappingTrainingDateRequest.SetValue(x => x.Id, _overlappingTrainingDateRequestRejectedEvent.OverlappingTrainingDateRequestId);
                _overlappingTrainingDateRequest.SetValue(x => x.DraftApprenticeship, _draftApprenticeship);
                _overlappingTrainingDateRequest.SetValue(x => x.PreviousApprenticeship, _apprenticeship);

                _db.DraftApprenticeships.Add(_draftApprenticeship);
                _db.Apprenticeships.Add(_apprenticeship);
                _db.OverlappingTrainingDateRequests.Add(_overlappingTrainingDateRequest);
                _db.SaveChanges();
            }
        }
    }
}
