using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services;

[TestFixture]
[Parallelizable]
public class CohortSupportStatusCalculatorTests
{
    [TestCase(EditStatus.Both, RequestSupportStatus.Approved)]
    [TestCase(EditStatus.Neither, RequestSupportStatus.None)]
    public void GetSupportStatus_WhenEditStatusIs(EditStatus editStatus, RequestSupportStatus expected)
    {
        var fixture = new CohortSupportStatusCalculatorTestsFixture();
        var result = fixture.Sut.GetStatus(editStatus, false, LastAction.None, Party.None, null, null);
        result.Should().Be(expected);
    }

    [TestCase(true, LastAction.None, RequestSupportStatus.SentToProvider)]
    [TestCase(false, LastAction.None, RequestSupportStatus.SentToProvider)]
    [TestCase(true, LastAction.AmendAfterRejected, RequestSupportStatus.None)]
    [TestCase(false, LastAction.AmendAfterRejected, RequestSupportStatus.SentToProvider)]
    [TestCase(true, LastAction.Amend, RequestSupportStatus.SentForReview)]
    [TestCase(false, LastAction.Amend, RequestSupportStatus.SentToProvider)]
    [TestCase(true, LastAction.Approve, RequestSupportStatus.WithProviderForApproval)]
    [TestCase(false, LastAction.Approve, RequestSupportStatus.SentToProvider)]
    public void GetSupportStatus_WhenEditStatusIsProviderOnly(bool hasApprentices, LastAction lastAction,
        RequestSupportStatus expected)
    {
        var fixture = new CohortSupportStatusCalculatorTestsFixture();
        var result = fixture.Sut.GetStatus(EditStatus.ProviderOnly, hasApprentices, lastAction, Party.None, null, null);
        result.Should().Be(expected);
    }

    [TestCase(true, LastAction.None, Party.None, RequestSupportStatus.NewRequest)]
    [TestCase(false, LastAction.None, Party.None, RequestSupportStatus.NewRequest)]
    [TestCase(true, LastAction.AmendAfterRejected, Party.None, RequestSupportStatus.NewRequest)]
    [TestCase(false, LastAction.AmendAfterRejected, Party.None, RequestSupportStatus.NewRequest)]
    [TestCase(true, LastAction.Amend, Party.None, RequestSupportStatus.ReadyForReview)]
    [TestCase(true, LastAction.Approve, Party.None, RequestSupportStatus.ReadyForReview)]
    [TestCase(true, LastAction.Approve, Party.Employer, RequestSupportStatus.ReadyForApproval)]
    [TestCase(true, LastAction.Amend, Party.Employer, RequestSupportStatus.None)]
    public void GetSupportStatus_WhenEditStatusIsEmployerOnly(bool hasApprentices, LastAction lastAction,
        Party partyStatus, RequestSupportStatus expected)
    {
        var fixture = new CohortSupportStatusCalculatorTestsFixture();
        var result =
            fixture.Sut.GetStatus(EditStatus.EmployerOnly, hasApprentices, lastAction, partyStatus, null, null);
        result.Should().Be(expected);
    }

    [Test]
    public void GetSupportStatus_WhenTransferSenderIdIsSetAndStatusIsApproved()
    {
        var fixture = new CohortSupportStatusCalculatorTestsFixture();
        var result = fixture.Sut.GetStatus(EditStatus.Both, true, LastAction.Approve, Party.None, 1,
            TransferApprovalStatus.Approved);
        result.Should().Be(RequestSupportStatus.None);
    }

    [Test]
    public void GetSupportStatus_WhenTransferSenderIdIsSetAndStatusIsRejected()
    {
        var fixture = new CohortSupportStatusCalculatorTestsFixture();
        var result = fixture.Sut.GetStatus(EditStatus.EmployerOnly, true, LastAction.Approve, Party.None, 1,
            TransferApprovalStatus.Rejected);
        result.Should().Be(RequestSupportStatus.RejectedBySender);
    }

    [TestCase(TransferApprovalStatus.Pending)]
    [TestCase(null)]
    public void GetSupportStatus_WhenEditStatusOfBothAndTransferStatusOf(TransferApprovalStatus? transferApprovalStatus)
    {
        var fixture = new CohortSupportStatusCalculatorTestsFixture();
        var result = fixture.Sut.GetStatus(EditStatus.Both, true, LastAction.Amend, Party.None, 1,
            transferApprovalStatus);
        result.Should().Be(RequestSupportStatus.WithSenderForApproval);
    }

    [TestCase(true, LastAction.None, RequestSupportStatus.SentToProvider)]
    [TestCase(false, LastAction.None, RequestSupportStatus.SentToProvider)]
    [TestCase(true, LastAction.AmendAfterRejected, RequestSupportStatus.None)]
    [TestCase(false, LastAction.AmendAfterRejected, RequestSupportStatus.SentToProvider)]
    [TestCase(true, LastAction.Amend, RequestSupportStatus.SentForReview)]
    [TestCase(false, LastAction.Amend, RequestSupportStatus.SentToProvider)]
    [TestCase(true, LastAction.Approve, RequestSupportStatus.WithProviderForApproval)]
    [TestCase(false, LastAction.Approve, RequestSupportStatus.SentToProvider)]
    public void GetSupportStatus_WhenEditStatusIsProviderOnlyWithTransferApprovalStatusOfPending(bool hasApprentices,
        LastAction lastAction, RequestSupportStatus expected)
    {
        var fixture = new CohortSupportStatusCalculatorTestsFixture();
        var result = fixture.Sut.GetStatus(EditStatus.ProviderOnly, hasApprentices, lastAction, Party.None, 1,
            TransferApprovalStatus.Pending);
        result.Should().Be(expected);
    }

    [TestCase(true, LastAction.None, Party.None, RequestSupportStatus.NewRequest)]
    [TestCase(false, LastAction.None, Party.None, RequestSupportStatus.NewRequest)]
    [TestCase(true, LastAction.AmendAfterRejected, Party.None, RequestSupportStatus.NewRequest)]
    [TestCase(false, LastAction.AmendAfterRejected, Party.None, RequestSupportStatus.NewRequest)]
    [TestCase(true, LastAction.Amend, Party.None, RequestSupportStatus.ReadyForReview)]
    [TestCase(true, LastAction.Approve, Party.None, RequestSupportStatus.ReadyForReview)]
    [TestCase(true, LastAction.Approve, Party.Employer, RequestSupportStatus.ReadyForApproval)]
    [TestCase(true, LastAction.Amend, Party.Employer, RequestSupportStatus.None)]
    public void GetSupportStatus_WhenEditStatusIsEmployerOnlyWithTransferApprovalStatusOfPending(bool hasApprentices,
        LastAction lastAction, Party partyStatus, RequestSupportStatus expected)
    {
        var fixture = new CohortSupportStatusCalculatorTestsFixture();
        var result = fixture.Sut.GetStatus(EditStatus.EmployerOnly, hasApprentices, lastAction, partyStatus, 1,
            TransferApprovalStatus.Pending);
        result.Should().Be(expected);
    }
}

internal class CohortSupportStatusCalculatorTestsFixture
{
    public CohortSupportStatusCalculator Sut;

    public CohortSupportStatusCalculatorTestsFixture()
    {

        Sut = new CohortSupportStatusCalculator();
    }
}