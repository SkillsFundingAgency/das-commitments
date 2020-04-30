﻿using System;
using System.Linq;
using AutoFixture;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.ChangeOfPartyRequest.SetCohortId
{
    [TestFixture]
    public class WhenCohortIdIsSet
    {
        private WhenCohortIdIsSetFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new WhenCohortIdIsSetFixture();
        }

        [Test]
        public void Then_CohortId_Property_Is_Updated()
        {
            _fixture.SetCohort();
            _fixture.VerifyCohortIdIsUpdated();
        }

        [Test]
        public void Then_State_Changes_Are_Tracked()
        {
            _fixture.SetCohort();
            _fixture.VerifyTracking();
        }

        [Test]
        public void Then_If_CohortId_Already_Set_Then_Throw()
        {
            _fixture.WithCohortIdAlreadySet();
            _fixture.SetCohort();
            _fixture.VerifyException();
        }

        private class WhenCohortIdIsSetFixture
        {
            private readonly CommitmentsV2.Models.ChangeOfPartyRequest _changeOfPartyRequest;
            private readonly CommitmentsV2.Models.Cohort _cohort;
            private Exception _exception;
            public UnitOfWorkContext UnitOfWorkContext { get; private set; }

            public WhenCohortIdIsSetFixture()
            {
                var autoFixture = new Fixture();

                UnitOfWorkContext = new UnitOfWorkContext();

                _cohort = new CommitmentsV2.Models.Cohort();
                _cohort.SetValue(x => x.Id, autoFixture.Create<long>());
                _cohort.SetValue(x => x.ProviderId, autoFixture.Create<long>());
                _cohort.SetValue(x => x.EmployerAccountId, autoFixture.Create<long>());

                _changeOfPartyRequest = autoFixture.Create<CommitmentsV2.Models.ChangeOfPartyRequest>();
            }

            public void SetCohort()
            {
                try
                {
                    _changeOfPartyRequest.SetCohort(_cohort, new UserInfo());
                }
                catch (Exception ex)
                {
                    _exception = ex;
                }
            }

            public void WithCohortIdAlreadySet()
            {
                _changeOfPartyRequest.SetValue(x => x.CohortId, _cohort.Id);
            }

            public void VerifyCohortIdIsUpdated()
            {
                Assert.AreEqual(_cohort.Id, _changeOfPartyRequest.CohortId);
            }

            public void VerifyTracking()
            {
                Assert.IsNotNull(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                                    && @event.EntityType ==
                                                                                    nameof(ChangeOfPartyRequest)));
            }

            public void VerifyException()
            {
                Assert.IsNotNull(_exception);
            }

            public void VerifyNoException()
            {
                Assert.IsNull(_exception);
            }
        }
    }
}
