using System;
using AutoFixture;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.ChangeOfPartyRequest.CreateCohort
{
    [TestFixture]
    public class WhenCohortIsCreated
    {
        private WhenCohortIsCreatedTestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new WhenCohortIsCreatedTestFixture();
        }

        [TestCase(ChangeOfPartyRequestType.ChangeEmployer)]
        [TestCase(ChangeOfPartyRequestType.ChangeProvider)]
        public void Then_It_Belongs_To_The_Correct_Provider(ChangeOfPartyRequestType requestType)
        {
            _fixture.WithChangeOfPartyType(requestType);
            _fixture.CreateCohort();
            _fixture.VerifyProvider();
        }

        [TestCase(ChangeOfPartyRequestType.ChangeEmployer)]
        [TestCase(ChangeOfPartyRequestType.ChangeProvider)]
        public void Then_It_Belongs_To_The_Correct_EmployerAccount(ChangeOfPartyRequestType requestType)
        {
            //EmployerAccount could be the one on the apprenticeship, or the one on the COPR itself, depending on the request type
            _fixture.WithChangeOfPartyType(requestType);
            _fixture.CreateCohort();
            _fixture.VerifyAccountId();
        }

        [TestCase(ChangeOfPartyRequestType.ChangeEmployer)]
        [TestCase(ChangeOfPartyRequestType.ChangeProvider)]
        public void Then_It_Belongs_To_The_Correct_EmployerAccountLegalEntity(ChangeOfPartyRequestType requestType)
        {
            _fixture.WithChangeOfPartyType(requestType);
            _fixture.CreateCohort();
            _fixture.VerifyAccountLegalEntityId();
        }

        [Test]
        public void Then_The_Originator_Is_Correct()
        {
            //originator is the one on the copr
            Assert.Fail();
        }

        [Test]
        public void Then_The_DraftApprenticeshipDetails_Are_Correct()
        {
            //Based on the original apprentice
            //And passed in res
            //Only 1
            Assert.Fail();
        }

        [Test]
        public void Then_WithParty_Is_Correct()
        {
            //with the originator of the request
            Assert.Fail();
        }

        [Test]
        public void Then_Originator_Approval_Is_Given()
        {
            //the approval of the originator is automatically set
            Assert.Fail();
        }


        [Test]
        public void Then_ChangeOfPartyRequestId_Is_Correct()
        {
            //the Id of the COPR is stored in the Cohort
            Assert.Fail();
        }

        [Test]
        public void Then_ChangeOfPartyCohortCreatedEvent_Is_Emitted()
        {
            //event pumped out
            Assert.Fail();
        }

        private class WhenCohortIsCreatedTestFixture
        {
            private Fixture _autoFixture = new Fixture();
            public CommitmentsV2.Models.Apprenticeship OriginalApprenticeship { get; private set; }
            public CommitmentsV2.Models.ChangeOfPartyRequest Request { get; private set; }
            public Guid ReservationId { get; set; }
            public CommitmentsV2.Models.Cohort Result { get; private set; }
            public Exception Exception { get; private set; }
            
            public WhenCohortIsCreatedTestFixture()
            {
                var cohort = new CommitmentsV2.Models.Cohort();
                cohort.SetValue(x => x.ProviderId, _autoFixture.Create<long>());

                OriginalApprenticeship = new CommitmentsV2.Models.Apprenticeship();
                OriginalApprenticeship.SetValue(x => x.Id, _autoFixture.Create<long>());
                OriginalApprenticeship.SetValue(x => x.Cohort, cohort);
                OriginalApprenticeship.SetValue(x => x.CommitmentId, cohort.Id);

                Request = new CommitmentsV2.Models.ChangeOfPartyRequest();
                Request.SetValue(x => x.Apprenticeship, OriginalApprenticeship);
                Request.SetValue(x => x.ApprenticeshipId, OriginalApprenticeship.Id);
            }

            public WhenCohortIsCreatedTestFixture WithChangeOfPartyType(ChangeOfPartyRequestType value)
            {
                Request.SetValue(x => x.ChangeOfPartyType, value);
                Request.SetValue(x => x.OriginatingParty, value == ChangeOfPartyRequestType.ChangeEmployer ? Party.Provider : Party.Employer);

                if (value == ChangeOfPartyRequestType.ChangeEmployer)
                {
                    var accountLegalEntity = new AccountLegalEntity();
                    accountLegalEntity.SetValue(x => x.Id, _autoFixture.Create<long>());
                    accountLegalEntity.SetValue(x => x.Account, new Account());
                    accountLegalEntity.Account.SetValue(x => x.Id, _autoFixture.Create<long>());
                    accountLegalEntity.SetValue(x => x.AccountId, accountLegalEntity.Account.Id);
                    Request.SetValue(x => x.AccountLegalEntity, accountLegalEntity);
                    Request.SetValue(x => x.AccountLegalEntityId, accountLegalEntity?.Id);
                    OriginalApprenticeship.Cohort.SetValue(x => x.ProviderId, _autoFixture.Create<long>());
                }
                else
                {
                    Request.SetValue(x => x.ProviderId, _autoFixture.Create<long>());
                    OriginalApprenticeship.Cohort.SetValue(x => x.AccountLegalEntityId, _autoFixture.Create<long>());
                    OriginalApprenticeship.Cohort.SetValue(x => x.EmployerAccountId, _autoFixture.Create<long>());
                }

                return this;
            }

            public void CreateCohort()
            {
                try
                {
                    Result = Request.CreateCohort(OriginalApprenticeship, ReservationId);
                }
                catch (Exception e)
                {
                    Exception = e;
                }
            }

            public void VerifyProvider()
            {
                //Provider could be the one on the apprenticeship, or the one on the COPR itself, depending on the request type
                Assert.AreEqual(
                    Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer
                                ? OriginalApprenticeship.Cohort.ProviderId
                                : Request.ProviderId, 
                        Result.ProviderId);
            }


            public void VerifyAccountId()
            {
                //Account could be the one on the apprenticeship, or the one on the COPR itself, depending on the request type
                Assert.AreEqual(
                    Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer
                        ? Request.AccountLegalEntity?.AccountId
                        : OriginalApprenticeship.Cohort.EmployerAccountId,
                    Result.EmployerAccountId);
            }


            public void VerifyAccountLegalEntityId()
            {
                //Ale could be the one on the apprenticeship, or the one on the COPR itself, depending on the request type
                Assert.AreEqual(
                    Request.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer
                        ? Request.AccountLegalEntityId.Value
                        : OriginalApprenticeship.Cohort.AccountLegalEntityId,
                    Result.AccountLegalEntityId);
            }
        }
    }
}
