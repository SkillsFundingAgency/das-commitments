using SFA.DAS.CommitmentsV2.Application.Queries.GetAllLearners;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetAllLearners
{
    [TestFixture]
    public class GetAllLearnersHandlerTests
    {
        private readonly GetAllLearnersQueryHandler _handler;
        private readonly ProviderCommitmentsDbContext _dbContext;

        /*
         * SQL to generate test data in C# format
         
            SELECT TOP 10
            CONCAT('new Apprenticeship { Id = ', Id, '') + 
            CONCAT(', FirstName = "', FirstName, '"') +
            CONCAT(', LastName = "', LastName, '"') +
            CONCAT(', StartDate = new DateTime(', FORMAT(StartDate, 'yyyy'), ', ', FORMAT(StartDate, 'MM'  ), ', ', FORMAT(StartDate, 'dd'  ), ')' ) +
            CONCAT(', EndDate = new DateTime(', FORMAT(EndDate, 'yyyy'), ', ', FORMAT(EndDate, 'MM'  ), ', ', FORMAT(EndDate, 'dd'  ), ')' ) +
            CONCAT(', CreatedOn = new DateTime(', FORMAT(CreatedOn, 'yyyy'), ', ', FORMAT(CreatedOn, 'MM'  ), ', ', FORMAT(CreatedOn, 'dd'  ), ')' ) +
            ' },'
            FROM Apprenticeship

         */
        private readonly List<Apprenticeship> Apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship { Id = 289438, FirstName = "Test", LastName = "Name8028125094", StartDate = new DateTime(2018, 07, 01), EndDate = new DateTime(2019, 10, 01), CreatedOn = new DateTime(2018, 07, 30) },
                new Apprenticeship { Id = 505412, FirstName = "Test", LastName = "Name9612111701", StartDate = new DateTime(2019, 03, 01), EndDate = new DateTime(2020, 06, 01), CreatedOn = new DateTime(2019, 03, 28) },
                new Apprenticeship { Id = 533717, FirstName = "Test", LastName = "Name1316758754", StartDate = new DateTime(2019, 04, 01), EndDate = new DateTime(2021, 04, 01), CreatedOn = new DateTime(2019, 05, 02) },
                new Apprenticeship { Id = 543913, FirstName = "Test", LastName = "Name9462140496", StartDate = new DateTime(2019, 07, 01), EndDate = new DateTime(2021, 06, 01), CreatedOn = new DateTime(2019, 05, 21) },
                new Apprenticeship { Id = 631487, FirstName = "Test", LastName = "Name3355170153", StartDate = new DateTime(2019, 09, 01), EndDate = new DateTime(2021, 11, 01), CreatedOn = new DateTime(2019, 09, 17) },
                new Apprenticeship { Id = 725441, FirstName = "Test", LastName = "Name6092998573", StartDate = new DateTime(2019, 11, 01), EndDate = new DateTime(2021, 11, 01), CreatedOn = new DateTime(2019, 11, 28) },
                new Apprenticeship { Id = 788611, FirstName = "Test", LastName = "Name9729801578", StartDate = new DateTime(2020, 02, 01), EndDate = new DateTime(2021, 05, 01), CreatedOn = new DateTime(2020, 02, 10) },
                new Apprenticeship { Id = 890154, FirstName = "Test", LastName = "Name7979537789", StartDate = new DateTime(2020, 09, 01), EndDate = new DateTime(2022, 03, 01), CreatedOn = new DateTime(2020, 08, 12) },
                new Apprenticeship { Id = 923289, FirstName = "Test", LastName = "Name3097091906", StartDate = new DateTime(2020, 10, 01), EndDate = new DateTime(2024, 04, 01), CreatedOn = new DateTime(2020, 09, 24) },
                new Apprenticeship { Id = 935805, FirstName = "Test", LastName = "Name2297563030", StartDate = new DateTime(2020, 10, 01), EndDate = new DateTime(2022, 09, 01), CreatedOn = new DateTime(2020, 10, 02) },
            };


        public GetAllLearnersHandlerTests()
        {
            _dbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options);
            _handler = new GetAllLearnersQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _dbContext));
        }

        [SetUp]
        public void Arrange()
        {
            _dbContext.Apprenticeships.AddRange(Apprenticeships);
            _dbContext.SaveChanges();
        }
        
        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _dbContext?.Dispose();
        }


        [Test]
        [Ignore("In-memory database in Core 2.2 does not support .FromSql")]
        public async Task When_GettingAllLearners_Then_LearnersAreMappedToLearnerResponse()
        {
            // Arrange.


            // Act.

            var result = await _handler.Handle(new GetAllLearnersQuery(null, 0, 0), CancellationToken.None);

            // Assert

            result.Learners.Should().NotBeNull();
            result.Learners.Should().HaveCount(Apprenticeships.Count);
        }
    }
}
