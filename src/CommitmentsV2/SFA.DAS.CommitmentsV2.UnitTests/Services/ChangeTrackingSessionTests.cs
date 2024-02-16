using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

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

        [Test]
        public void Complete_TrackInsert_Emits_Event()
        {
            _fixture
                .TrackInsert()
                .Complete();

            _fixture.VerifyInsertEvent();
        }


        [Test]
        public void Complete_TrackUpdate_Emits_Event()
        {
            _fixture
                .TrackUpdate()
                .Complete();

            _fixture.VerifyUpdateEvent();
        }

        [Test]
        public void Complete_TrackDelete_Emits_Event()
        {
            _fixture
                .TrackDelete()
                .Complete();

            _fixture.VerifyDeleteEvent();
        }

        private class ChangeTrackingSessionTestsFixture
        {
            public ChangeTrackingSession ChangeTrackingSession { get; set; }
            public TestTrackableEntity TestInsertTrackableEntity { get; set; }
            public TestTrackableEntity TestUpdateTrackableEntity { get; set; }
            public TestTrackableEntity TestDeleteTrackableEntity { get; set; }
            public Dictionary<string, object> TestUpdateInitialState { get; set; }
            public Dictionary<string, object> TestDeleteInitialState { get; set; }
            public Dictionary<string, object> TestInsertUpdatedState { get; set; }
            public Dictionary<string, object> TestUpdateUpdatedState { get; set; }
            public UnitOfWorkContext UnitOfWorkContext { get; set; }
            public Mock<IStateService> StateService { get; set; }
            public Party Party { get; set; }
            public UserInfo UserInfo { get; set; }
            public UserAction UserAction { get; set; }
            public long ProviderId { get; set; }
            public long EmployerAccountId { get; set; }

            public ChangeTrackingSessionTestsFixture()
            {
                UnitOfWorkContext = new UnitOfWorkContext();

                var autoFixture = new Fixture();

                ProviderId = autoFixture.Create<long>();
                EmployerAccountId = autoFixture.Create<long>();
                UserInfo = autoFixture.Create<UserInfo>();
                UserAction = autoFixture.Create<UserAction>();
                Party = autoFixture.Create<Party>();

                TestUpdateTrackableEntity = new TestTrackableEntity(autoFixture.Create<long>());
                TestInsertTrackableEntity = new TestTrackableEntity(autoFixture.Create<long>());
                TestDeleteTrackableEntity = new TestTrackableEntity(autoFixture.Create<long>());

                TestUpdateInitialState = autoFixture.Create<Dictionary<string, object>>();
                TestDeleteInitialState = autoFixture.Create<Dictionary<string, object>>();
                TestInsertUpdatedState = autoFixture.Create<Dictionary<string, object>>();
                TestUpdateUpdatedState = autoFixture.Create<Dictionary<string, object>>();

                StateService = new Mock<IStateService>();
                StateService.Setup(x => x.GetState(It.Is<object>(o => o == TestUpdateTrackableEntity))).Returns(TestUpdateInitialState);
                StateService.Setup(x => x.GetState(It.Is<object>(o => o == TestDeleteTrackableEntity))).Returns(TestDeleteInitialState);

                ChangeTrackingSession = new ChangeTrackingSession(StateService.Object, UserAction, Party, EmployerAccountId, ProviderId, UserInfo);
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

            public ChangeTrackingSessionTestsFixture Complete()
            {
                StateService.Setup(x => x.GetState(It.Is<object>(o => o == TestUpdateTrackableEntity))).Returns(TestUpdateUpdatedState);
                StateService.Setup(x => x.GetState(It.Is<object>(o => o == TestInsertTrackableEntity))).Returns(TestInsertUpdatedState);

                ChangeTrackingSession.CompleteTrackingSession();
                return this;
            }

            public void VerifyInsertedItemInitialStateIsNull()
            {
                var trackedItem = ChangeTrackingSession.TrackedItems.Single(x => x.Operation == ChangeTrackingOperation.Insert);
                Assert.That(trackedItem.InitialState, Is.EqualTo(null));
            }

            public void VerifyUpdateInitialState()
            {
                var trackedItem = ChangeTrackingSession.TrackedItems.Single(x => x.Operation == ChangeTrackingOperation.Update);
                Assert.That(trackedItem.InitialState, Is.SameAs(TestUpdateInitialState));
            }

            public void VerifyDeletedInitialState()
            {
                var trackedItem = ChangeTrackingSession.TrackedItems.Single(x => x.Operation == ChangeTrackingOperation.Delete);
                Assert.That(trackedItem.InitialState, Is.SameAs(TestDeleteInitialState));
            }

            public void VerifyInsertEvent()
            {
                Assert.That(UnitOfWorkContext.GetEvents().OfType<EntityStateChangedEvent>().Single(x =>
                    x.ProviderId == ProviderId &&
                    x.EmployerAccountId == EmployerAccountId &&
                    x.EntityId == TestInsertTrackableEntity.Id &&
                    x.EntityType == TestInsertTrackableEntity.GetType().Name &&
                    x.InitialState == null &&
                    x.StateChangeType == UserAction &&
                    x.UpdatedState == JsonConvert.SerializeObject(TestInsertUpdatedState) &&
                    x.UpdatingParty == Party &&
                    x.UpdatingUserId == UserInfo.UserId &&
                    x.UpdatingUserName == UserInfo.UserDisplayName
                    ), Is.Not.Null);
            }

            public void VerifyUpdateEvent()
            {
                Assert.That(UnitOfWorkContext.GetEvents().OfType<EntityStateChangedEvent>().Single(x =>
                    x.ProviderId == ProviderId &&
                    x.EmployerAccountId == EmployerAccountId &&
                    x.EntityId == TestUpdateTrackableEntity.Id &&
                    x.EntityType == TestUpdateTrackableEntity.GetType().Name &&
                    x.InitialState == JsonConvert.SerializeObject(TestUpdateInitialState) &&
                    x.StateChangeType == UserAction &&
                    x.UpdatedState == JsonConvert.SerializeObject(TestUpdateUpdatedState) &&
                    x.UpdatingParty == Party &&
                    x.UpdatingUserId == UserInfo.UserId &&
                    x.UpdatingUserName == UserInfo.UserDisplayName
                ), Is.Not.Null);
            }

            public void VerifyDeleteEvent()
            {
                Assert.That(UnitOfWorkContext.GetEvents().OfType<EntityStateChangedEvent>().Single(x =>
                    x.ProviderId == ProviderId &&
                    x.EmployerAccountId == EmployerAccountId &&
                    x.EntityId == TestDeleteTrackableEntity.Id &&
                    x.EntityType == TestDeleteTrackableEntity.GetType().Name &&
                    x.InitialState == JsonConvert.SerializeObject(TestDeleteInitialState) &&
                    x.StateChangeType == UserAction &&
                    x.UpdatedState == null &&
                    x.UpdatingParty == Party &&
                    x.UpdatingUserId == UserInfo.UserId &&
                    x.UpdatingUserName == UserInfo.UserDisplayName
                ), Is.Not.Null);
            }
        }

        private class TestTrackableEntity : ITrackableEntity
        {
            public TestTrackableEntity(long id)
            {
                Id = id;
            }
            public long Id { get; }
        }

    }
}
