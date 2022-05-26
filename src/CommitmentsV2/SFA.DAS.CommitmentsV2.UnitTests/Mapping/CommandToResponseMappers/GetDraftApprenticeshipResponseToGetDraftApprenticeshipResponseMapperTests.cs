using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Mapping.CommandToResponseMappers;
using SFA.DAS.CommitmentsV2.Types;
using HttpResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses;
using CommandResponse = SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeship;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.CommandToResponseMappers
{
    [TestFixture]
    public class GetDraftApprenticeshipResponseToGetDraftApprenticeshipResponseMapperTests : OldMapperTester<GetDraftApprenticeshipResponseToGetDraftApprenticeshipResponseMapper, CommandResponse.GetDraftApprenticeshipQueryResult, HttpResponse.GetDraftApprenticeshipResponse>
    {
        [Test]
        public Task Map_FirstName_ShouldBeSet()
        {
            return AssertPropertySet(input => input.FirstName, "Fred");
        }

        [Test]
        public Task Map_LastName_ShouldBeSet()
        {
            return AssertPropertySet(input => input.LastName, "Flintstone");
        }

        [Test]
        public Task Map_Email_ShouldBeSet()
        {
            return AssertPropertySet(input => input.Email, "ringo@star");
        }

        [Test]
        public Task Map_DateOfBirth_ShouldBeSet()
        {
            return AssertPropertySet(input => input.DateOfBirth, (DateTime?) DateTime.Now);
        }

        [Test]
        public Task Map_Uln_ShouldBeSet()
        {
            return AssertPropertySet(input => input.Uln, "1234567890");
        }

        [Test]
        public Task Map_CourseCode_ShouldBeSet()
        {
            return AssertPropertySet(input => input.CourseCode, "ABC123");
        }

        [TestCase(DeliveryModel.Regular)]
        [TestCase(DeliveryModel.PortableFlexiJob)]
        public async Task Map_DeliveryModel_ShouldBeSet(DeliveryModel dm)
        {
            var mapper = CreateMapper();
            var from = new CommandResponse.GetDraftApprenticeshipQueryResult
            {
                DeliveryModel = dm
            };

            var to = await mapper.Map(from);
            Assert.AreEqual(dm, to.DeliveryModel);
        }

        [Test]
        public Task Map_Cost_ShouldBeSet()
        {
            return AssertPropertySet(input => input.Cost, (int?)123);
        }

        [Test]
        public Task Map_EmploymentEndDate_ShouldBeSet()
        {
            return AssertPropertySet(input => input.EmploymentEndDate, (DateTime?)DateTime.Now);
        }

        [Test]
        public Task Map_EmploymentPrice_ShouldBeSet()
        {
            return AssertPropertySet(input => input.EmploymentPrice, (int?)123);
        }

        [Test]
        public Task Map_StartDate_ShouldBeSet()
        {
            return AssertPropertySet(input => input.StartDate, (DateTime?) DateTime.Now);
        }

        [Test]
        public Task Map_EndDate_ShouldBeSet()
        {
            return AssertPropertySet(input => input.EndDate, (DateTime?) DateTime.Now);
        }

        [Test]
        public Task Map_OriginatorReference_ShouldBeSet()
        {
            return AssertPropertySet(input => input.Reference, "XYZ456");
        }

        [Test]
        public Task Map_ReservationId_ShouldBeSet()
        {
            Guid reservationId = Guid.NewGuid();
            return AssertPropertySet(input => input.ReservationId, (Guid?) Guid.NewGuid());
        }

        [Test]
        public Task Map_IsContinuation_ShouldBeSet()
        {
            return AssertPropertySet(input => input.IsContinuation, true);
        }

        [Test]
        public Task Map_OriginalStartDate_ShouldBeSet()
        {
            DateTime? originalStartDate = new DateTime(2020, 10, 1);
            return AssertPropertySet(input => input.OriginalStartDate, originalStartDate);
        }
        
        [Test]
        public Task Map_StandardUId_ShouldBeSet()
        {
            var standardUId = "IdValue";
            return AssertPropertySet(input => input.StandardUId, standardUId);
        }
                
        [Test]
        public Task Map_StandardHasOptions_ShouldBeSet()
        {
            return AssertPropertySet(input => input.HasStandardOptions, true);
        }

        [TestCase(true)]
        [TestCase(false)]
        [TestCase(null)]
        public Task Map_RecognisePriorLearning_ShouldBeSet(bool? value)
        {
            return AssertPropertySet(input => input.RecognisePriorLearning, value);
        }

        [Test]
        public Task Map_DurationReducedBy_ShouldBeSet()
        {
            var durationReducedBy = 9;
            return AssertPropertySet(input => input.DurationReducedBy, durationReducedBy);
        }

        [Test]
        public Task Map_PriceReducedBy_ShouldBeSet()
        {
            var priceReducedBy = 9;
            return AssertPropertySet(input => input.PriceReducedBy, priceReducedBy);
        }
    }
}