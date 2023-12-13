using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Kernel;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Mapping.ResponseMappers;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.ResponseMappers
{
    [TestFixture]
    public class UpdateDraftApprenticeshipRequestToUpdateDraftApprenticeshipCommandMapperTests
    {
        private readonly GetApprenticeshipUpdateResponseMapper _mapper;
        private GetApprenticeshipUpdateQueryResult _source;
        private GetApprenticeshipUpdatesResponse _result;

        public UpdateDraftApprenticeshipRequestToUpdateDraftApprenticeshipCommandMapperTests()
        {
            _mapper = new GetApprenticeshipUpdateResponseMapper();
        }

        [SetUp]
        public async Task Arrange()
        {
            var autoFixture = new Fixture();
            autoFixture.Customizations.Add(new ApprenticeshipUpdateOriginatorSpecimenBuilder());
            _source = autoFixture.Create<GetApprenticeshipUpdateQueryResult>();
            _result = await _mapper.Map(TestHelper.Clone(_source));
        }

        [Test]
        public void ApprenticeshipUpdatesAreMappedCorrectly()
        {
            var compare = new CompareLogic(new ComparisonConfig { IgnoreObjectTypes = true, MaxDifferences = 100 });
            var compareResult = compare.Compare(_source.ApprenticeshipUpdates, _result.ApprenticeshipUpdates);
            Assert.IsTrue(compareResult.AreEqual);
        }

        [Test]
        public void ApprenticeshipUpdates_PartyIsMappedProperly()
        {
            foreach (var source in _source.ApprenticeshipUpdates)
            {
                var result = _result.ApprenticeshipUpdates.First(x => x.Id == source.Id);
                Assert.That(result.OriginatingParty, Is.EqualTo(source.Originator.ToParty()));
            }
        }

        [Test]
        public void ApprenticeshipUpdates_DeliveryPartyIsMappedCorrectly()
        {
            foreach (var source in _source.ApprenticeshipUpdates)
            {
                var result = _result.ApprenticeshipUpdates.First(x => x.Id == source.Id);
                Assert.That(result.DeliveryModel, Is.EqualTo(source.DeliveryModel));
            }
        }
    }

    public class ApprenticeshipUpdateOriginatorSpecimenBuilder :
    ISpecimenBuilder
    {
        public object Create(object request,
            ISpecimenContext context)
        {
            var pi = request as PropertyInfo;

            if (pi == null)
            {
                return new NoSpecimen();
            }
            if (pi == typeof(Originator)
                || pi.Name == "Originator")
            {
                var enums = Enum.GetValues(typeof(Originator)).Cast<Originator>().Where(x => x != Originator.Unknown);
                return enums.ElementAt((new Random()).Next(0, enums.Count()));
            }

            return new NoSpecimen();
        }
    }

}
