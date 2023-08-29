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
            Assert.IsTrue(compareResult.AreEqual);
        }

        [Test]
        public void DeliveryModelIsMappedCorrectly()
        {
            Assert.AreEqual(_source.DeliveryModel, _result.DeliveryModel);
        }

        [Test]
        public void EmploymentPriceIsMappedCorrectly()
        {
            Assert.AreEqual(_source.FlexibleEmployment.EmploymentPrice, _result.EmploymentPrice);
        }

        [Test]
        public void EmploymentEndDateIsMappedCorrectly()
        {
            Assert.AreEqual(_source.FlexibleEmployment.EmploymentEndDate, _result.EmploymentEndDate);
        }

        [Test]
        public void RecognisePriorLearningIsMappedCorrectly()
        {
            Assert.AreEqual(_source.RecognisePriorLearning, _result.RecognisePriorLearning);
        }

        [Test]
        public void DurationReducedByIsMappedCorrectly()
        {
            Assert.AreEqual(_source.ApprenticeshipPriorLearning.DurationReducedBy, _result.DurationReducedBy);
        }

        [Test]
        public void PriceReducedByIsMappedCorrectly()
        {
            Assert.AreEqual(_source.ApprenticeshipPriorLearning.PriceReducedBy, _result.PriceReducedBy);
        }

        [Test]
        public void DurationReducedByHoursIsMappedCorrectly()
        {
            Assert.AreEqual(_source.ApprenticeshipPriorLearning.DurationReducedByHours, _result.DurationReducedByHours);
        }

        [Test]
        public void WeightageReducedByIsMappedCorrectly()
        {
            Assert.AreEqual(_source.ApprenticeshipPriorLearning.WeightageReducedBy, _result.WeightageReducedBy);
        }

        [Test]
        public void ReasonForRplReductionIsMappedCorrectly()
        {
            Assert.AreEqual(_source.ApprenticeshipPriorLearning.ReasonForRplReduction, _result.ReasonForRplReduction);
        }

        [Test]
        public void QualificationsForRplReductionIsMappedCorrectly()
        {
            Assert.AreEqual(_source.ApprenticeshipPriorLearning.QualificationsForRplReduction, _result.QualificationsForRplReduction);
        }

        [Test]
        public void StartDateIsMappedCorrectly()
        {
            Assert.AreEqual(_source.ActualStartDate, _result.ActualStartDate);
        }

        [Test]
        public void IsOnFlexiPaymentPilotIsMappedCorrectly()
        {
            Assert.AreEqual(_source.IsOnFlexiPaymentPilot, _result.IsOnFlexiPaymentPilot);
        }

        [Test]
        public void TrainingPriceIsMappedCorrectly()
        {
            Assert.AreEqual(_source.TrainingPrice, _result.TrainingPrice);
        }

        [Test]
        public void EndPointAssessmentPriceIsMappedCorrectly()
        {
            Assert.AreEqual(_source.EndPointAssessmentPrice, _result.EndPointAssessmentPrice);
        }
    }
}