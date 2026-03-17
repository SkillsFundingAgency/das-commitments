using KellermanSoftware.CompareNetObjects;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Mapping.ResponseMappers;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.ResponseMappers;

[TestFixture]
public class GetApprenticeshipResponseMapperTests
{
    private readonly GetApprenticeshipResponseMapper _mapper = new();
    private GetApprenticeshipQueryResult _source;
    private GetApprenticeshipResponse _result;

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
        var config = new ComparisonConfig { IgnoreObjectTypes = true };
        config.MembersToIgnore.Add("GetApprenticeshipQueryResult.EmployerVerificationStatus");
        config.MembersToIgnore.Add("GetApprenticeshipResponse.EmployerVerificationStatus");
        var compare = new CompareLogic(config);
        var compareResult = compare.Compare(_source, _result);
        compareResult.AreEqual.Should().BeTrue();
        _result.EmployerVerificationStatus.Should().Be((int?)_source.EmployerVerificationStatus);
    }

    [Test]
    public void DeliveryModelIsMappedCorrectly()
    {
        _result.DeliveryModel.Should().Be(_source.DeliveryModel);
    }

    [Test]
    public void EmploymentPriceIsMappedCorrectly()
    {
        _result.EmploymentPrice.Should().Be(_source.FlexibleEmployment.EmploymentPrice);
    }

    [Test]
    public void EmploymentEndDateIsMappedCorrectly()
    {
        _result.EmploymentEndDate.Should().Be(_source.FlexibleEmployment.EmploymentEndDate);
    }

    [Test]
    public void RecognisePriorLearningIsMappedCorrectly()
    {
        _result.RecognisePriorLearning.Should().Be(_source.RecognisePriorLearning);
    }

    [Test]
    public void DurationReducedByIsMappedCorrectly()
    {
        _result.DurationReducedBy.Should().Be(_source.ApprenticeshipPriorLearning.DurationReducedBy);
    }

    [Test]
    public void PriceReducedByIsMappedCorrectly()
    {
        _result.PriceReducedBy.Should().Be(_source.ApprenticeshipPriorLearning.PriceReducedBy);
    }

    [Test]
    public void DurationReducedByHoursIsMappedCorrectly()
    {
        _result.DurationReducedByHours.Should().Be(_source.ApprenticeshipPriorLearning.DurationReducedByHours);
    }

    [Test]
    public void TrainingTotalHoursIsMappedCorrectly()
    {
        _result.TrainingTotalHours.Should().Be(_source.TrainingTotalHours);
    }

    [Test]
    public void IsDurationReducedIsMappedCorrectly()
    {
        _result.IsDurationReducedByRpl.Should().Be(_source.ApprenticeshipPriorLearning.IsDurationReducedByRpl);
    }

    [Test]
    public void StartDateIsMappedCorrectly()
    {
        _result.ActualStartDate.Should().Be(_source.ActualStartDate);
    }

    [Test]
    public void IsDurationReducedByRplIsMappedCorrectly()
    {
        _result.IsDurationReducedByRpl.Should().Be(_source.ApprenticeshipPriorLearning?.IsDurationReducedByRpl);
    }

    [Test]
    public async Task EmployerVerificationStatusAndNotesAreMappedCorrectly()
    {
        _source.EmployerVerificationStatus = EmployerVerificationRequestStatus.Passed;
        _source.EmployerVerificationNotes = null;

        _result = await _mapper.Map(TestHelper.Clone(_source));

        _result.EmployerVerificationStatus.Should().Be((int)EmployerVerificationRequestStatus.Passed);
        _result.EmployerVerificationNotes.Should().BeNull();
    }

    [Test]
    public async Task EmployerVerificationStatusAndNotesAreMappedCorrectly_WhenErrorWithNotes()
    {
        _source.EmployerVerificationStatus = EmployerVerificationRequestStatus.Error;
        _source.EmployerVerificationNotes = "PAYENotFound";

        _result = await _mapper.Map(TestHelper.Clone(_source));

        _result.EmployerVerificationStatus.Should().Be((int)EmployerVerificationRequestStatus.Error);
        _result.EmployerVerificationNotes.Should().Be("PAYENotFound");
    }

    [Test]
    public async Task EmployerVerificationStatusAndNotesAreNullWhenNotSet()
    {
        _source.EmployerVerificationStatus = null;
        _source.EmployerVerificationNotes = null;

        _result = await _mapper.Map(TestHelper.Clone(_source));

        _result.EmployerVerificationStatus.Should().BeNull();
        _result.EmployerVerificationNotes.Should().BeNull();
    }
}