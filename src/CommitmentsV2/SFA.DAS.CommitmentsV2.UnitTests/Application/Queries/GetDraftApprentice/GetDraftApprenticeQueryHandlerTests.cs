using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprentice;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.UnitOfWork;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetDraftApprentice
{
    [TestFixture]
    [Parallelizable]
    public class GetDraftApprenticeHandlerTests
    {
        [TestCase(Party.Employer, "EMPREF123", Party.Employer, "EMPREF123")]
        [TestCase(Party.Employer, "EMPREF123", Party.Provider, null)]
        [TestCase(Party.Provider, "PROVREF123", Party.Provider, "PROVREF123")]
        [TestCase(Party.Provider, "PROVREF123", Party.Employer, null)]
        public async Task Handle_WhenRequested_ThenShouldReturnCorrectReferenceForRequester(Party creatingParty, string creatingReference, Party requestingParty, string expectedReference)
        {
            var fixture = new GetDraftApprenticeHandlerTestFixtures()
                .SetApprentice(creatingParty, creatingReference)
                .SetRequestingParty(requestingParty);

            var result = await fixture.Handle(); 
            
            Assert.AreEqual(expectedReference, result.Reference);
        }
    }

    public class GetDraftApprenticeHandlerTestFixtures
    {
        public CommitmentsDbContext Db { get; set; }
        public Mock<IAuthenticationService> AuthenticationServiceMock { get; set; }
        public GetDraftApprenticeHandler Handler { get; set; }

        private long CohortId = 1;
        private long ApprenticeshipId = 1;

        public GetDraftApprenticeHandlerTestFixtures()
        {
            AuthenticationServiceMock = new Mock<IAuthenticationService>();
            Db = new CommitmentsDbContext(new DbContextOptionsBuilder<CommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            Handler = new GetDraftApprenticeHandler(
                new Lazy<CommitmentsDbContext>(() => Db), 
                AuthenticationServiceMock.Object);
        }

        public Task<GetDraftApprenticeResponse> Handle()
        {
            var query = new GetDraftApprenticeRequest(CohortId, ApprenticeshipId);
            return Handler.Handle(query, CancellationToken.None);
        }

        public GetDraftApprenticeHandlerTestFixtures SetRequestingParty(Party requestingParty)
        {
            AuthenticationServiceMock
                .Setup(a => a.GetUserParty())
                .Returns(requestingParty);

            return this;
        }

        public GetDraftApprenticeHandlerTestFixtures SetApprentice(Party creatingParty, string reference)
        {
            // This line is required.
            // ReSharper disable once ObjectCreationAsStatement
            new UnitOfWorkContext();

            var draftApprenticeshipDetails = new DraftApprenticeshipDetails
            {
                Reference = reference,
                Id = ApprenticeshipId,
                FirstName = "AFirstName",
                LastName = "ALastName"
            };

            var commitment = new Cohort(
                new Provider(),
                new AccountLegalEntity(),
                draftApprenticeshipDetails,
                creatingParty,
                new UserInfo());

            Db.Cohorts.Add(commitment);

            Db.SaveChanges();

            ApprenticeshipId = commitment.Apprenticeships.First().Id;
            CohortId = commitment.Id;

            return this;
        }
    }
}