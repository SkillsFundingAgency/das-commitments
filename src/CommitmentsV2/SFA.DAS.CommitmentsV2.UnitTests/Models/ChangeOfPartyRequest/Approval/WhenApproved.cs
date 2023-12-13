using System;
using System.Linq;
using AutoFixture;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.ChangeOfPartyRequest.Approval
{
    [TestFixture]
    public class WhenApproved
    {
        private WhenApprovedTestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new WhenApprovedTestFixture();
        }

        [Test]
        public void Then_Status_Is_Set_To_Approved()
        {
            _fixture.Approve();
            _fixture.VerifyStatusIsApproved();
        }

        [Test]
        public void Then_ActionedOn_Is_Set()
        {
            _fixture.Approve();
            _fixture.VerifyActionedOn();
        }

        [Test]
        public void Then_State_Changes_Are_Tracked()
        {
            _fixture.Approve();
            _fixture.VerifyTracking();
        }

        [TestCase(ChangeOfPartyRequestStatus.Approved)]
        [TestCase(ChangeOfPartyRequestStatus.Rejected)]
        [TestCase(ChangeOfPartyRequestStatus.Withdrawn)]
        public void Then_If_Status_Is_Invalid_Then_Throws(ChangeOfPartyRequestStatus status)
        {
            _fixture.WithStatus(status);
            _fixture.Approve();
            _fixture.VerifyException();
        }

        private class WhenApprovedTestFixture
        {
            private readonly CommitmentsV2.Models.ChangeOfPartyRequest _changeOfPartyRequest;
            private readonly UserInfo _userInfo;
            private Exception _exception;
            private readonly UnitOfWorkContext _unitOfWorkContext;

            public WhenApprovedTestFixture()
            {
                var autoFixture = new Fixture();
                _unitOfWorkContext = new UnitOfWorkContext();
                _changeOfPartyRequest = autoFixture.Create<CommitmentsV2.Models.ChangeOfPartyRequest>();
                _changeOfPartyRequest.SetValue(x => x.OriginatingParty, Party.Provider);
                _changeOfPartyRequest.SetValue(x => x.Status, ChangeOfPartyRequestStatus.Pending);
                _changeOfPartyRequest.SetValue(x => x.Cohort, new CommitmentsV2.Models.Cohort());
                _userInfo = autoFixture.Create<UserInfo>();
            }

            public WhenApprovedTestFixture WithStatus(ChangeOfPartyRequestStatus status)
            {
                _changeOfPartyRequest.SetValue(x => x.Status, status);
                return this;
            }

            public void Approve()
            {
                try
                {
                    _changeOfPartyRequest.Approve(_changeOfPartyRequest.OriginatingParty.GetOtherParty(), _userInfo);
                }
                catch (Exception ex)
                {
                    _exception = ex;
                }
            }

            public void VerifyStatusIsApproved()
            {
                Assert.That(_changeOfPartyRequest.Status, Is.EqualTo(ChangeOfPartyRequestStatus.Approved));
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
