using System.Threading.Tasks;
using AutoFixture;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Mapping.ResponseMappers;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.ResponseMappers
{
    [TestFixture]
    public class GetApprenticeshipResponseMapperTests
    {
        private readonly GetApprenticeshipResponseMapper _mapper;
        private GetApprenticeshipQueryResult _source;
        private GetApprenticeshipResponse _result;

        public GetApprenticeshipResponseMapperTests()
        {
            _mapper = new GetApprenticeshipResponseMapper();
        }

        [SetUp]
        public async Task Arrange()
        {
            var autoFixture = new Fixture();

            _source = autoFixture.Build<GetApprenticeshipQueryResult>()
                .With(e => e.DeliveryModel, DeliveryModel.PortableFlexiJob)
                .With(e => e.ApprenticeshipPriorLearning, autoFixture.Create<ApprenticeshipPriorLearning>())
                .Create();

            _result = await _mapper.Map(TestHelper.Clone(_source));
        }

        [Test]
        public void ResponseIsMappedCorrectly()
        {
            var compare = new CompareLogic(new ComparisonConfig { IgnoreObjectTypes = true });
            var compareResult = compare.Compare(_source, _result);
            Assert.That(compareResult.AreEqual, Is.True);
        }

        [Test]
        public void DeliveryModelIsMappedCorrectly()
        {
            Assert.That(_result.DeliveryModel, Is.EqualTo(_source.DeliveryModel));
        }

        [Test]
        public void EmploymentPriceIsMappedCorrectly()
        {
            Assert.That(_result.EmploymentPrice, Is.EqualTo(_source.FlexibleEmployment.EmploymentPrice));
        }

        [Test]
        public void EmploymentEndDateIsMappedCorrectly()
        {
            Assert.That(_result.EmploymentEndDate, Is.EqualTo(_source.FlexibleEmployment.EmploymentEndDate));
        }

        [Test]
        public void RecognisePriorLearningIsMappedCorrectly()
        {
            Assert.That(_result.RecognisePriorLearning, Is.EqualTo(_source.RecognisePriorLearning));
        }

        [Test]
        public void DurationReducedByIsMappedCorrectly()
        {
            Assert.That(_result.DurationReducedBy, Is.EqualTo(_source.ApprenticeshipPriorLearning.DurationReducedBy));
        }

        [Test]
        public void PriceReducedByIsMappedCorrectly()
        {
            Assert.That(_result.PriceReducedBy, Is.EqualTo(_source.ApprenticeshipPriorLearning.PriceReducedBy));
        }

        [Test]
        public void DurationReducedByHoursIsMappedCorrectly()
        {
            Assert.That(_result.DurationReducedByHours, Is.EqualTo(_source.ApprenticeshipPriorLearning.DurationReducedByHours));
        }

        [Test]
        public void TrainingTotalHoursIsMappedCorrectly()
        {
            Assert.That(_result.TrainingTotalHours, Is.EqualTo(_source.TrainingTotalHours));
        }

        [Test]
        public void IsDurationReducedIsMappedCorrectly()
        {
            Assert.That(_result.IsDurationReducedByRpl, Is.EqualTo(_source.ApprenticeshipPriorLearning.IsDurationReducedByRpl));
        }

        [Test]
        public void StartDateIsMappedCorrectly()
        {
            Assert.That(_result.ActualStartDate, Is.EqualTo(_source.ActualStartDate));
        }

        [Test]
        public void IsOnFlexiPaymentPilotIsMappedCorrectly()
        {
            Assert.That(_result.IsOnFlexiPaymentPilot, Is.EqualTo(_source.IsOnFlexiPaymentPilot));
        }

        [Test]
        public void IsDurationReducedByRplIsMappedCorrectly()
        {
            Assert.That(_result.IsDurationReducedByRpl, Is.EqualTo(_source.ApprenticeshipPriorLearning?.IsDurationReducedByRpl));
        }
    }
}