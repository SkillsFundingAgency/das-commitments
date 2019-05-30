﻿using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Mapping.RequestToCommandMappers;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.RequestToCommandMappers
{
    [TestFixture()]
    public class CreateCohortRequestToAddCohortCommandMapperTests : MapperTester<CreateCohortRequestToAddCohortCommandMapper, CreateCohortRequest, AddCohortCommand>
    {
        [Test]
        public Task Map_AccountLegalEntityId_ShouldBeSet()
        {
            const long accountLegalEntityIdId = 123;
            return AssertPropertySet(input => input.AccountLegalEntityId = accountLegalEntityIdId, output => output.AccountLegalEntityId == accountLegalEntityIdId);
        }

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

        [Test]
        public Task Map_StartDate_ShouldBeSet()
        {
            DateTime startDate = DateTime.Now;
            return AssertPropertySet(input => input.StartDate = startDate, output => output.StartDate == startDate);
        }
    }
}
