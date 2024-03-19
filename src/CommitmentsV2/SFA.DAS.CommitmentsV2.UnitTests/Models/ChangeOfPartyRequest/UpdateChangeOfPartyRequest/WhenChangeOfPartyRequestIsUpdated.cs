using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using SFA.DAS.CommitmentsV2;

using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.UnitOfWork.Context;
using AutoFixture;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.ChangeOfPartyRequest.UpdateChangeOfPartyRequest
{
    [TestFixture]
    public class WhenChangeOfPartyRequestIsUpdated
    {
        private WhenChangeOfPartyRequestIsUpdatedFixture _fixture;
        
        [SetUp]
        public void Arrange()
        {
            _fixture = new WhenChangeOfPartyRequestIsUpdatedFixture();
        }

        [Test]
        public void Then_PriceIsUpdated()
        {
            _fixture.UpdateChangeOfPartyRequest();

            _fixture.VerifyPriceIsUpdated();
        }


        [Test]
        public void Then_StartDateIsUpdated()
        {
            _fixture.UpdateChangeOfPartyRequest();

            _fixture.VerifyStartDateIsUpdated();
        }

        [Test]
        public void Then_EndDateIsUpdated()
        {
            _fixture.UpdateChangeOfPartyRequest();

            _fixture.VerifyEndDateIsUpdated();
        }

        [Test]
        public void Then_PriceIsSetToNull_When_Draft_Apprenticeship_CostIsNull()
        {
            _fixture.SetPrice(null);
            _fixture.UpdateChangeOfPartyRequest();

            _fixture.VerifyPriceIsUpdated();
        }

        private class WhenChangeOfPartyRequestIsUpdatedFixture
        {
            private readonly CommitmentsV2.Models.ChangeOfPartyRequest _changeOfPartyRequest;
            private readonly CommitmentsV2.Models.DraftApprenticeship _draftApprenticeship;

            private readonly long _providerId;
            private readonly long _employerAccountId;

            public UnitOfWorkContext UnitOfWorkContext { get; private set; }

            public WhenChangeOfPartyRequestIsUpdatedFixture()
            {
                var autoFixture = new Fixture();

                _providerId = autoFixture.Create<long>();
                _employerAccountId = autoFixture.Create<long>();

                _changeOfPartyRequest = autoFixture.Create<CommitmentsV2.Models.ChangeOfPartyRequest>();

                _draftApprenticeship = new CommitmentsV2.Models.DraftApprenticeship();
                _draftApprenticeship.SetValue(x => x.Cost, autoFixture.Create<decimal>());
                _draftApprenticeship.SetValue(x => x.StartDate, autoFixture.Create<DateTime>());
                _draftApprenticeship.SetValue(x => x.EndDate, autoFixture.Create<DateTime>());

                UnitOfWorkContext = new UnitOfWorkContext();
            }

            public void SetPrice(decimal? price)
            {
                _draftApprenticeship.SetValue(x => x.Cost, price);
            }

            public void UpdateChangeOfPartyRequest()
            {
                _changeOfPartyRequest.UpdateChangeOfPartyRequest(_draftApprenticeship, _employerAccountId, _providerId, new UserInfo(), Party.Provider);
            }

            public void VerifyPriceIsUpdated()
            {
                Assert.That(_changeOfPartyRequest.Price, Is.EqualTo(_draftApprenticeship.Cost));
            }

            public void VerifyStartDateIsUpdated()
            {
                Assert.That(_changeOfPartyRequest.StartDate, Is.EqualTo(_draftApprenticeship.StartDate));
            }

            public void VerifyEndDateIsUpdated()
            {
                Assert.That(_changeOfPartyRequest.EndDate, Is.EqualTo(_draftApprenticeship.EndDate));
            }
        }
    }
}
