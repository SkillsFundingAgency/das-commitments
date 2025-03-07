﻿using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.ChangeOfPartyRequest.Rejection
{
    [TestFixture]
    public class WhenRejected
    {
        private WhenRejectedTestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new WhenRejectedTestFixture();
        }

        [Test]
        public void Then_Status_Is_Set_To_Rejected()
        {
            _fixture.Reject();
            _fixture.VerifyStatusIsRejected();
        }

        [Test]
        public void Then_ActionedOn_Is_Set()
        {
            _fixture.Reject();
            _fixture.VerifyActionedOn();
        }

        [Test]
        public void Then_State_Changes_Are_Tracked()
        {
            _fixture.Reject();
            _fixture.VerifyTracking();
        }

        [TestCase(ChangeOfPartyRequestStatus.Approved)]
        [TestCase(ChangeOfPartyRequestStatus.Rejected)]
        [TestCase(ChangeOfPartyRequestStatus.Withdrawn)]
        public void Then_If_Status_Is_Invalid_Then_Throws(ChangeOfPartyRequestStatus status)
        {
            _fixture.WithStatus(status);
            _fixture.Reject();
            _fixture.VerifyException();
        }

        private class WhenRejectedTestFixture
        {
            private readonly CommitmentsV2.Models.ChangeOfPartyRequest _changeOfPartyRequest;
            private readonly UserInfo _userInfo;
            private Exception _exception;
            private readonly UnitOfWorkContext _unitOfWorkContext;

            public WhenRejectedTestFixture()
            {
                var autoFixture = new Fixture();
                _unitOfWorkContext = new UnitOfWorkContext();
                _changeOfPartyRequest = autoFixture.Create<CommitmentsV2.Models.ChangeOfPartyRequest>();
                _changeOfPartyRequest.SetValue(x => x.OriginatingParty, Party.Provider);
                _changeOfPartyRequest.SetValue(x => x.Status, ChangeOfPartyRequestStatus.Pending);
                _changeOfPartyRequest.SetValue(x => x.Cohort, new CommitmentsV2.Models.Cohort());
                _userInfo = autoFixture.Create<UserInfo>();
            }

            public WhenRejectedTestFixture WithStatus(ChangeOfPartyRequestStatus status)
            {
                _changeOfPartyRequest.SetValue(x => x.Status, status);
                return this;
            }

            public void Reject()
            {
                try
                {
                    _changeOfPartyRequest.Reject(_changeOfPartyRequest.OriginatingParty.GetOtherParty(),_userInfo);
                }
                catch (Exception ex)
                {
                    _exception = ex;
                }
            }

            public void VerifyStatusIsRejected()
            {
                Assert.That(_changeOfPartyRequest.Status, Is.EqualTo(ChangeOfPartyRequestStatus.Rejected));
            }

            public void VerifyActionedOn()
            {
                Assert.That(_changeOfPartyRequest.ActionedOn, Is.Not.Null);
            }

            public void VerifyTracking()
            {
                Assert.That(_unitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                                    && @event.EntityType ==
                                                                                    nameof(ChangeOfPartyRequest)), Is.Not.Null);
            }

            public void VerifyException()
            {
                Assert.That(_exception, Is.Not.Null);
            }
        }
    }
}
