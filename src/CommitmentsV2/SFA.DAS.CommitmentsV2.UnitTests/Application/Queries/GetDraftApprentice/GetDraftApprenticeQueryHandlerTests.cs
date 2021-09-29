using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
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
            result.HasStandardOptions.Should().BeFalse();
        }

        [Test]
        public async Task Then_If_There_Options_With_The_Standard_Property_Is_True()
        {
            var fixture = new GetDraftApprenticeHandlerTestFixtures()
                .SetApprentice(Party.Employer, "EMPREF123", true)
                .SetRequestingParty(Party.Employer);

            var result = await fixture.Handle();

            result.HasStandardOptions.Should().BeTrue();
        }
    }

    public class GetDraftApprenticeHandlerTestFixtures
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        public Mock<IAuthenticationService> AuthenticationServiceMock { get; set; }
        public GetDraftApprenticeshipQueryHandler Handler { get; set; }

        private long CohortId = 1;
        private long ApprenticeshipId = 1;

        public GetDraftApprenticeHandlerTestFixtures()
        {
            AuthenticationServiceMock = new Mock<IAuthenticationService>();
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            Handler = new GetDraftApprenticeshipQueryHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db), 
                AuthenticationServiceMock.Object);
        }

        public Task<GetDraftApprenticeshipQueryResult> Handle()
        {
            var query = new GetDraftApprenticeshipQuery(CohortId, ApprenticeshipId);
            return Handler.Handle(query, CancellationToken.None);
        }

        public GetDraftApprenticeHandlerTestFixtures SetRequestingParty(Party requestingParty)
        {
            AuthenticationServiceMock
                .Setup(a => a.GetUserParty())
                .Returns(requestingParty);

            return this;
        }

        public GetDraftApprenticeHandlerTestFixtures SetApprentice(Party creatingParty, string reference, bool hasOptions = false)
        {
            // This line is required.
            // ReSharper disable once ObjectCreationAsStatement
            new UnitOfWorkContext();

            var autoFixture = new Fixture();

            var draftApprenticeshipDetails = new DraftApprenticeshipDetails
            {
                Reference = reference,
                Id = ApprenticeshipId,
                FirstName = "AFirstName",
                LastName = "ALastName"
            };

            if (hasOptions)
            {
                draftApprenticeshipDetails.StandardUId = "ST1.01";

                Db.StandardOptions.Add(new StandardOption
                {
                    StandardUId = "ST1.01",
                    Option = "An option"
                });
            }

            var commitment = new Cohort(
                autoFixture.Create<long>(),
                autoFixture.Create<long>(),
                autoFixture.Create<long>(),
                null,
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