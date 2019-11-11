using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public class ChangeTrackingSessionTests
    {
        private ChangeTrackingSessionTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ChangeTrackingSessionTestsFixture();
        }

        [Test]
        public void TrackInsert_InitialState_IsNull()
        {
            _fixture.TrackInsert();
            _fixture.VerifyInsertedItemInitialStateIsNull();
        }

        [Test]
        public void TrackUpdate_InitialState_Is_Captured()
        {
            _fixture.TrackUpdate();
            _fixture.VerifyUpdateInitialState();
        }

        [Test]
        public void TrackDelete_InitialState_Is_Captured()
        {
            _fixture.TrackDelete();
            _fixture.VerifyDeletedInitialState();
        }

        private class ChangeTrackingSessionTestsFixture
        {
            public ChangeTrackingSession ChangeTrackingSession { get; set; }
            public TestTrackableEntity TestInsertTrackableEntity { get; set; }
            public TestTrackableEntity TestUpdateTrackableEntity { get; set; }
            public TestTrackableEntity TestDeleteTrackableEntity { get; set; }
            public Dictionary<string, object> TestUpdateInitialState { get; set; }
            public Dictionary<string, object> TestDeleteInitialState { get; set; }


            public ChangeTrackingSessionTestsFixture()
            {
                var stateService = new Mock<IStateService>();
                stateService.Setup(x => x.GetState(It.Is<object>(o => o == TestUpdateTrackableEntity))).Returns(TestUpdateInitialState);
                stateService.Setup(x => x.GetState(It.Is<object>(o => o == TestDeleteTrackableEntity))).Returns(TestDeleteInitialState);

                ChangeTrackingSession = new ChangeTrackingSession(stateService.Object, UserAction.AddDraftApprenticeship, Party.Employer, 1, 2, new UserInfo());

                TestInsertTrackableEntity = new TestTrackableEntity();
            }

            public ChangeTrackingSessionTestsFixture TrackInsert()
            {
                ChangeTrackingSession.TrackInsert(TestInsertTrackableEntity);
                return this;
            }

            public ChangeTrackingSessionTestsFixture TrackUpdate()
            {
                ChangeTrackingSession.TrackUpdate(TestUpdateTrackableEntity);
                return this;
            }

            public ChangeTrackingSessionTestsFixture TrackDelete()
            {
                ChangeTrackingSession.TrackDelete(TestDeleteTrackableEntity);
                return this;
            }

            public void VerifyInsertedItemInitialStateIsNull()
            {
                var trackedItem = ChangeTrackingSession.TrackedItems.Single(x => x.Operation == ChangeTrackingOperation.Insert);
                Assert.IsTrue(trackedItem.InitialState == null);
            }

            public void VerifyUpdateInitialState()
            {
                var trackedItem = ChangeTrackingSession.TrackedItems.Single(x => x.Operation == ChangeTrackingOperation.Update);
                Assert.AreSame(TestUpdateInitialState, trackedItem.InitialState);
            }

            public void VerifyDeletedInitialState()
            {
                var trackedItem = ChangeTrackingSession.TrackedItems.Single(x => x.Operation == ChangeTrackingOperation.Delete);
                Assert.AreSame(TestDeleteInitialState, trackedItem.InitialState);
            }
        }

        private class TestTrackableEntity : ITrackableEntity
        {
            public long Id { get; }
        }

    }
}
