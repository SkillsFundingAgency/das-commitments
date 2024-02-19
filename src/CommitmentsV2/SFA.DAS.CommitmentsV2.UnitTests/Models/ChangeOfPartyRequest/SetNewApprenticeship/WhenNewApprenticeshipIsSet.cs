using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.ChangeOfPartyRequest.SetNewApprenticeship
{
    [TestFixture]
    public class WhenNewApprenticeshipIsSet
    {
        private WhenNewApprenticeshipIsSetFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new WhenNewApprenticeshipIsSetFixture();
        }

        [Test]
        public void Then_NewApprenticeshipId_Property_Is_Updated()
        {
            _fixture.SetNewApprenticeship();
            _fixture.VerifyNewApprenticeshipIdIsUpdated();
        }

        [Test]
        public void Then_State_Changes_Are_Tracked()
        {
            _fixture.SetNewApprenticeship();
            _fixture.VerifyTracking();
        }

        [Test]
        public void Then_If_NewApprenticeshipId_Already_Set_Then_Throw()
        {
            _fixture.WithNewApprenticeshipIdAlreadySet();
            _fixture.SetNewApprenticeship();
            _fixture.VerifyException();
        }

        private class WhenNewApprenticeshipIsSetFixture
        {
            private readonly CommitmentsV2.Models.ChangeOfPartyRequest _changeOfPartyRequest;
            private readonly CommitmentsV2.Models.Cohort _cohort;
            private readonly CommitmentsV2.Models.Apprenticeship _apprenticeship;
            private Exception _exception;
            public UnitOfWorkContext UnitOfWorkContext { get; private set; }

            public WhenNewApprenticeshipIsSetFixture()
            {
                var autoFixture = new Fixture();

                UnitOfWorkContext = new UnitOfWorkContext();

                _cohort = new CommitmentsV2.Models.Cohort();
                _cohort.SetValue(x => x.Id, autoFixture.Create<long>());
                _cohort.SetValue(x => x.ProviderId, autoFixture.Create<long>());
                _cohort.SetValue(x => x.EmployerAccountId, autoFixture.Create<long>());

                _apprenticeship = new CommitmentsV2.Models.Apprenticeship();
                _apprenticeship.SetValue(x => x.Id, autoFixture.Create<long>());
                _apprenticeship.SetValue(x => x.Cohort, _cohort);

                _changeOfPartyRequest = autoFixture.Create<CommitmentsV2.Models.ChangeOfPartyRequest>();
            }

            public void SetNewApprenticeship()
            {
                try
                {
                    _changeOfPartyRequest.SetNewApprenticeship(_apprenticeship, new UserInfo(), Party.Employer);
                }
                catch (Exception ex)
                {
                    _exception = ex;
                }
            }

            public void WithNewApprenticeshipIdAlreadySet()
            {
                _changeOfPartyRequest.SetValue(x => x.NewApprenticeshipId, _cohort.Id);
            }

            public void VerifyNewApprenticeshipIdIsUpdated()
            {
                Assert.That(_changeOfPartyRequest.NewApprenticeshipId, Is.EqualTo(_apprenticeship.Id));
            }

            public void VerifyTracking()
            {
                Assert.That(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                                    && @event.EntityType ==
                                                                                    nameof(ChangeOfPartyRequest)), Is.Not.Null);
            }

            public void VerifyException()
            {
                Assert.That(_exception, Is.Not.Null);
            }

            public void VerifyNoException()
            {
                Assert.That(_exception, Is.Null);
            }
        }
    }
}
