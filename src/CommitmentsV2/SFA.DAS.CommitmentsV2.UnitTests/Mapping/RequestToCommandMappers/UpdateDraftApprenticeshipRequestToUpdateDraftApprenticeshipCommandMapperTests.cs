using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Mapping.RequestToCommandMappers;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.RequestToCommandMappers
{
    [TestFixture]
    public class UpdateDraftApprenticeshipRequestToUpdateDraftApprenticeshipCommandMapperTests : 
        OldMapperTester<UpdateDraftApprenticeshipRequestToUpdateDraftApprenticeshipCommandMapper,
        UpdateDraftApprenticeshipRequest, 
        UpdateDraftApprenticeshipCommand>
    {
        [Test]
        public Task Map_CourseCode_ShouldBeSet()
        {
            return AssertPropertySet(from => from.CourseCode, "001/AAA");
        }
        
        [Test]
        public Task Map_CostWithoutValue_ShouldBeSet()
        {
            return AssertPropertySet(from => from.Cost, (int?)null);
        }

        [Test]
        public Task Map_CostWithValue_ShouldBeSet()
        {
            return AssertPropertySet(from => from.Cost, (int?)1234);
        }

        [Test]
        public Task Map_EmploymentPriceWithoutValue_ShouldBeSet()
        {
            return AssertPropertySet(from => from.EmploymentPrice, (int?)null);
        }

        [Test]
        public Task Map_EmploymentPriceWithValue_ShouldBeSet()
        {
            return AssertPropertySet(from => from.EmploymentPrice, (int?)1234);
        }

        [Test]
        public Task Map_StartDateWithoutValue_ShouldBeSet()
        {
            return AssertPropertySet(from => from.StartDate, (DateTime?)null);
        }

        [Test]
        public Task Map_StartDateWithValue_ShouldBeSet()
        {
            return AssertPropertySet(from => from.StartDate, (DateTime?)DateTime.Now);
        }

        [Test]
        public Task Map_EndDateWithoutValue_ShouldBeSet()
        {
            return AssertPropertySet(from => from.EndDate, (DateTime?)null);
        }

        [Test]
        public Task Map_EndDateWithValue_ShouldBeSet()
        {
            return AssertPropertySet(from => from.EndDate, (DateTime?)DateTime.Now);
        }

        [Test]
        public Task Map_EmploymentEndDateWithoutValue_ShouldBeSet()
        {
            return AssertPropertySet(from => from.EmploymentEndDate, (DateTime?)null);
        }

        [Test]
        public Task Map_EmploymentEndDateWithValue_ShouldBeSet()
        {
            return AssertPropertySet(from => from.EmploymentEndDate, (DateTime?)DateTime.Now);
        }

        [Test]
        public Task Map_ReferenceWithValue_ShouldBeSet()
        {
            return AssertPropertySet(from => from.Reference, "ABC123");
        }

        [Test]
        public Task Map_ReferenceWithoutValue_ShouldNotBeSet()
        {
            return AssertPropertySet(from => from.Reference, (string)null);
        }

        [Test]
        public Task Map_ReservationIdWithValue_ShouldBeSet()
        {
            return AssertPropertySet(from => from.ReservationId, (Guid?)Guid.NewGuid());
        }

        [Test]
        public Task Map_ReservationIdWithoutValue_ShouldNotBeSet()
        {
            return AssertPropertySet(from => from.ReservationId, (Guid?)null);
        }

        [Test]
        public Task Map_FirstName_ShouldBeSet()
        {
            return AssertPropertySet(from => from.FirstName, "Fred");
        }

        [Test]
        public Task Map_LastName_ShouldBeSet()
        {
            return AssertPropertySet(from => from.LastName, "Flintstone");
        }

        [Test]
        public Task Map_DateOfBirthWithoutValue_ShouldBeSet()
        {
            return AssertPropertySet(from => from.DateOfBirth, (DateTime?)null);
        }

        [Test]
        public Task Map_DateOfBirthWithValue_ShouldBeSet()
        {
            return AssertPropertySet(from => from.DateOfBirth, (DateTime?)DateTime.Now);
        }

        [Test]
        public Task Map_Uln_ShouldBeSet()
        {
            return AssertPropertySet(from => from.Uln, "1234567890");
        }

        [TestCase(DeliveryModel.Regular)]
        [TestCase(DeliveryModel.PortableFlexiJob)]
        public Task Map_DeliveryModel_ShouldBeSet(DeliveryModel dm)
        {
            return AssertPropertySet(from => from.DeliveryModel, dm);
        }
    }
}
