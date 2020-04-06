using System;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Apprenticeship
{
    [TestFixture]
    public class WhenCreatingChangeOfPartyRequest
    {
        private WhenCreatingChangeOfPartyRequestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new WhenCreatingChangeOfPartyRequestFixture();
        }

        [Test]
        public void ThenChangeOfPartyRequestIsCreated()
        {
            _fixture.CreateChangeOfPartyRequest();
            _fixture.VerifyResult();
        }

        private class WhenCreatingChangeOfPartyRequestFixture
        {
            public CommitmentsV2.Models.Apprenticeship Apprenticeship { get; }
            public CommitmentsV2.Models.ChangeOfPartyRequest Result { get; private set; }
            public UnitOfWorkContext UnitOfWorkContext { get; private set; }

            public WhenCreatingChangeOfPartyRequestFixture()
            {
                UnitOfWorkContext = new UnitOfWorkContext();

                Apprenticeship = new CommitmentsV2.Models.Apprenticeship
                {
                    Cohort = new CommitmentsV2.Models.Cohort()
                };
            }

            public void CreateChangeOfPartyRequest()
            {
                Result = Apprenticeship.CreateChangeOfPartyRequest(ChangeOfPartyRequestType.ChangeEmployer, Party.Provider, 1,
                    1000, DateTime.UtcNow, DateTime.UtcNow, new UserInfo());
            }

            public void VerifyResult()
            {
                Assert.IsNotNull(Result);
            }
        }
    }
}
