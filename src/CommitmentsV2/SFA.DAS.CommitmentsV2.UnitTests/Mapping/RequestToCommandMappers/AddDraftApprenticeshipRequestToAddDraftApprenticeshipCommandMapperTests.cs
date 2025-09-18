using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Mapping.RequestToCommandMappers;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.RequestToCommandMappers
{
    [TestFixture]
    public class AddDraftApprenticeshipRequestToAddDraftApprenticeshipCommandMapperTests : OldMapperTester<AddDraftApprenticeshipRequestToAddDraftApprenticeshipCommandMapper, AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand>
    {
        [Test]
        public Task Map_ProviderId_ShouldBeSet()
        {
            const long providerId = 123;
            return AssertPropertySet(input => input.ProviderId = providerId, output => output.ProviderId == providerId);
        }

        [Test]
        public Task Map_Cost_ShouldBeSet()
        {
            const int cost = 789;
            return AssertPropertySet(input => input.Cost = cost, output => output.Cost == cost);
        }

        [Test]
        public Task Map_TrainingPrice_ShouldBeSet()
        {
            const int value = 654687687;
            return AssertPropertySet(input => input.TrainingPrice = value, output => output.TrainingPrice == value);
        }

        [Test]
        public Task Map_EndPointAssessmentPrice_ShouldBeSet()
        {
            const int value = 787865649;
            return AssertPropertySet(input => input.EndPointAssessmentPrice = value, output => output.EndPointAssessmentPrice == value);
        }

        [Test]
        public Task Map_CourseCode_ShouldBeSet()
        {
            const string courseCode = "ABC123";
            return AssertPropertySet(input => input.CourseCode = courseCode, output => output.CourseCode == courseCode);
        }

        [Test]
        public Task Map_EndDate_ShouldBeSet()
        {
            DateTime endDate = DateTime.Now;
            return AssertPropertySet(input => input.EndDate = endDate, output => output.EndDate == endDate);
        }

        [Test]
        public Task Map_OriginatorReference_ShouldBeSet()
        {
            const string originatorReference = "Foo379";
            return AssertPropertySet(input => input.OriginatorReference = originatorReference, output => output.OriginatorReference == originatorReference);
        }

        [Test]
        public Task Map_ReservationId_ShouldBeSet()
        {
            Guid reservationId = Guid.NewGuid();
            return AssertPropertySet(input => input.ReservationId = reservationId, output => output.ReservationId == reservationId);
        }

        [TestCase(null)]
        [TestCase(12345)]
        public Task Map_LearnerDataId_ShouldBeSet(long? learnerDataId)
        {
            return AssertPropertySet(input => input.LearnerDataId = learnerDataId, output => output.LearnerDataId == learnerDataId);
        }

        [Test]
        public Task Map_StartDate_ShouldBeSet()
        {
            DateTime startDate = DateTime.Now;
            return AssertPropertySet(input => input.StartDate = startDate, output => output.StartDate == startDate);
        }

        [Test]
        public Task Map_ActualStartDate_ShouldBeSet()
        {
            DateTime startDate = DateTime.Now;
            return AssertPropertySet(input => input.ActualStartDate = startDate, output => output.ActualStartDate == startDate);
        }

        [TestCase(DeliveryModel.Regular)]
        [TestCase(DeliveryModel.PortableFlexiJob)]
        public Task Map_DeliveryModel_ShouldBeSet(DeliveryModel dm)
        {
            return AssertPropertySet(input => input.DeliveryModel = dm, output => output.DeliveryModel == dm);
        }

        [Test]
        public Task Map_EmploymentEndDate_ShouldBeSet()
        {
            DateTime employmentEndDate = DateTime.Now;
            return AssertPropertySet(input => input.EmploymentEndDate = employmentEndDate, output => output.EmploymentEndDate == employmentEndDate);
        }

        [Test]
        public Task Map_EmploymentPrice_ShouldBeSet()
        {
            int employmentPrice = 456;
            return AssertPropertySet(input => input.EmploymentPrice = employmentPrice, output => output.EmploymentPrice == employmentPrice);
        }

        [Test]
        public Task Map_RequestingParty_ShouldBeSet()
        {
            Party requestingParty = Party.Provider;
            return AssertPropertySet(input => input.RequestingParty = requestingParty, output => output.RequestingParty == requestingParty);
        }
    }
}