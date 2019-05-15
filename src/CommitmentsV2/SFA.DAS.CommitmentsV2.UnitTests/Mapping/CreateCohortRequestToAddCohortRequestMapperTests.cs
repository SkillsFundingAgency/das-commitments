using System;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Mapping.RequestToCommandMappers;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping
{
    [TestFixture()]
    public class CreateCohortRequestToAddCohortRequestMapperTests
    {
        [Test]
        public void Map_AccountLegalEntityId_ShouldBeSet()
        {
            const long accountLegalEntityIdId = 123;
            AssertPropertySet(input => input.AccountLegalEntityId = accountLegalEntityIdId, output => output.AccountLegalEntityId == accountLegalEntityIdId);
        }

        [Test]
        public void Map_ProviderId_ShouldBeSet()
        {
            const long providerId = 123;
            AssertPropertySet(input => input.ProviderId = providerId, output => output.ProviderId == providerId);
        }

        [Test]
        public void Map_Cost_ShouldBeSet()
        {
            const int cost = 789;
            AssertPropertySet(input => input.Cost = cost, output => output.Cost == cost);
        }

        [Test]
        public void Map_CourseCode_ShouldBeSet()
        {
            const string courseCode = "ABC123";
            AssertPropertySet(input => input.CourseCode = courseCode, output => output.CourseCode == courseCode);
        }

        [Test]
        public void Map_EndDate_ShouldBeSet()
        {
            DateTime endDate = DateTime.Now;
            AssertPropertySet(input => input.EndDate = endDate, output => output.EndDate == endDate);
        }

        [Test]
        public void Map_OriginatorReference_ShouldBeSet()
        {
            const string originatorReference = "Foo379";
            AssertPropertySet(input => input.OriginatorReference = originatorReference, output => output.OriginatorReference == originatorReference);
        }

        [Test]
        public void Map_ReservationId_ShouldBeSet()
        {
            Guid reservationId = Guid.NewGuid();
            AssertPropertySet(input => input.ReservationId = reservationId, output => output.ReservationId == reservationId);
        }

        [Test]
        public void Map_StartDate_ShouldBeSet()
        {
            DateTime startDate = DateTime.Now;
            AssertPropertySet(input => input.StartDate = startDate, output => output.StartDate == startDate);
        }

        private void AssertPropertySet(Action<CreateCohortRequest> setInput, Func<AddCohortCommand, bool> expectOutput)
        {
            var mapper = new CreateCohortRequestToAddCohortCommandMapper();

            var input = new CreateCohortRequest();

            setInput(input);

            var output = mapper.Map(input);

            Assert.IsTrue(expectOutput(output));
        }
    }
}
