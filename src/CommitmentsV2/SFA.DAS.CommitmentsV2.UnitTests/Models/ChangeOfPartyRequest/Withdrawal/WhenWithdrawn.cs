﻿using System;
using System.Linq;
using AutoFixture;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.ChangeOfPartyRequest.Withdrawal
{
    [TestFixture]
    public class WhenWithdrawn
    {
        private WhenWithdrawnTestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new WhenWithdrawnTestFixture();
        }

        [Test]
        public void Then_Status_Is_Set_To_Withdrawn()
        {
            _fixture.Withdraw();
            _fixture.VerifyStatusIsWithdrawn();
        }

        [Test]
        public void Then_ActionedOn_Is_Set()
        {
            _fixture.Withdraw();
            _fixture.VerifyActionedOn();
        }

        [Test]
        public void Then_State_Changes_Are_Tracked()
        {
            _fixture.Withdraw();
            _fixture.VerifyTracking();
        }

        [TestCase(ChangeOfPartyRequestStatus.Approved)]
        [TestCase(ChangeOfPartyRequestStatus.Rejected)]
        [TestCase(ChangeOfPartyRequestStatus.Withdrawn)]
        public void Then_If_Status_Is_Invalid_Then_Throws(ChangeOfPartyRequestStatus status)
        {
            _fixture.WithStatus(status);
            _fixture.Withdraw();
            _fixture.VerifyException();
        }

        private class WhenWithdrawnTestFixture
        {
            private readonly CommitmentsV2.Models.ChangeOfPartyRequest _changeOfPartyRequest;
            private readonly UserInfo _userInfo;
            private Exception _exception;
            private readonly UnitOfWorkContext _unitOfWorkContext;

            public WhenWithdrawnTestFixture()
            {
                var autoFixture = new Fixture();
                _unitOfWorkContext = new UnitOfWorkContext();
                _changeOfPartyRequest = autoFixture.Create<CommitmentsV2.Models.ChangeOfPartyRequest>();
                _changeOfPartyRequest.SetValue(x => x.OriginatingParty, Party.Provider);
                _changeOfPartyRequest.SetValue(x => x.Status, ChangeOfPartyRequestStatus.Pending);
                _changeOfPartyRequest.SetValue(x => x.Cohort, new CommitmentsV2.Models.Cohort());
                _userInfo = autoFixture.Create<UserInfo>();
            }

            public WhenWithdrawnTestFixture WithStatus(ChangeOfPartyRequestStatus status)
            {
                _changeOfPartyRequest.SetValue(x => x.Status, status);
                return this;
            }

            public void Withdraw()
            {
                try
                {
                    _changeOfPartyRequest.Withdraw(_changeOfPartyRequest.OriginatingParty, _userInfo);
                }
                catch (Exception ex)
                {
                    _exception = ex;
                }
            }

            public void VerifyStatusIsWithdrawn()
            {
                Assert.AreEqual(ChangeOfPartyRequestStatus.Withdrawn, _changeOfPartyRequest.Status);
            }

            public void VerifyActionedOn()
            {
                Assert.IsNotNull(_changeOfPartyRequest.ActionedOn);
            }

            public void VerifyTracking()
            {
                Assert.IsNotNull(_unitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                                    && @event.EntityType ==
                                                                                    nameof(ChangeOfPartyRequest)));
            }

            public void VerifyException()
            {
                Assert.IsNotNull(_exception);
            }
        }
    }
}
