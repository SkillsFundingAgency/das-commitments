﻿using AutoFixture;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Domain.DraftApprenticeship
{
    [TestFixture]
    public class WhenDraftApprenticeshipIsUpdated
    {
        private DraftApprenticeshipUpdateTestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new DraftApprenticeshipUpdateTestFixture();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenFirstNameIsMappedCorrectly(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).ApplyUpdate();
            Assert.AreEqual(_fixture.DraftApprenticeshipDetails.FirstName, result.FirstName);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenLastNameIsMappedCorrectly(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).ApplyUpdate();
            Assert.AreEqual(_fixture.DraftApprenticeshipDetails.LastName, result.LastName);
        }

        [Test]
        public void ThenUlnIsMappedCorrectly()
        {
            var result = _fixture.WithModifyingParty(Party.Provider).WithUlnUpdateOnly().ApplyUpdate();
            Assert.AreEqual(_fixture.DraftApprenticeshipDetails.Uln, result.Uln);
        }

        [Test]
        public void ThenEmployerUlnUpdateIsNotAllowed()
        {
            Assert.Throws<DomainException>(() => _fixture.WithModifyingParty(Party.Employer).WithPriorApprovalByOtherParty().WithUlnUpdateOnly().ApplyUpdate());
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenCostIsMappedCorrectly(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).ApplyUpdate();
            Assert.AreEqual(_fixture.DraftApprenticeshipDetails.Cost, result.Cost);
        }


        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenStartDateIsMappedCorrectly(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).ApplyUpdate();
            Assert.AreEqual(_fixture.DraftApprenticeshipDetails.StartDate, result.StartDate);
        }


        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenEndDateIsMappedCorrectly(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).ApplyUpdate();
            Assert.AreEqual(_fixture.DraftApprenticeshipDetails.EndDate, result.EndDate);
        }


        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenDateOfBirthIsMappedCorrectly(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).ApplyUpdate();
            Assert.AreEqual(_fixture.DraftApprenticeshipDetails.DateOfBirth, result.DateOfBirth);
        }


        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenProgrammeTypeIsMappedCorrectly(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).ApplyUpdate();
            Assert.AreEqual(_fixture.DraftApprenticeshipDetails.TrainingProgramme.ProgrammeType, result.ProgrammeType);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenCourseCodeIsMappedCorrectly(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).ApplyUpdate();
            Assert.AreEqual(_fixture.DraftApprenticeshipDetails.TrainingProgramme.CourseCode, result.CourseCode);
        }


        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenCourseNameIsMappedCorrectly(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).ApplyUpdate();
            Assert.AreEqual(_fixture.DraftApprenticeshipDetails.TrainingProgramme.Name, result.CourseName);
        }

        [Test]
        public void ThenProviderReferenceIsMappedCorrectly()
        {
            var result = _fixture.WithModifyingParty(Party.Provider).ApplyUpdate();
            Assert.AreEqual(_fixture.DraftApprenticeshipDetails.Reference, result.ProviderRef);
        }

        [Test]
        public void ThenEmployerReferenceIsMappedCorrectly()
        {
            var result = _fixture.WithModifyingParty(Party.Employer).ApplyUpdate();
            Assert.AreEqual(_fixture.DraftApprenticeshipDetails.Reference, result.EmployerRef);

        }

        [Test]
        public void ThenPriorApprovalByEmployerIsNotResetByUlnUpdate()
        {
            var result = _fixture.WithModifyingParty(Party.Provider).WithPriorApprovalByOtherParty().WithUlnUpdateOnly().ApplyUpdate();
            Assert.AreNotEqual(AgreementStatus.NotAgreed, result.AgreementStatus);
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenPriorApprovalByOtherPartyIsNotResetByReferenceUpdate(Party modifyingParty)
        {
            var result = _fixture.WithModifyingParty(modifyingParty).WithPriorApprovalByOtherParty().WithReferenceUpdateOnly().ApplyUpdate();
            Assert.AreNotEqual(AgreementStatus.NotAgreed, result.AgreementStatus);
        }

        private class DraftApprenticeshipUpdateTestFixture
        {
            private readonly Fixture _autoFixture;
            private readonly CommitmentsV2.Models.DraftApprenticeship _draftApprenticeship;
            public DraftApprenticeshipDetails DraftApprenticeshipDetails { get; private set; }
            private CommitmentsV2.Models.DraftApprenticeship _result;
            private Party _modifyingParty;

            public DraftApprenticeshipUpdateTestFixture()
            {
                _autoFixture = new Fixture();
                _draftApprenticeship = new CommitmentsV2.Models.DraftApprenticeship(_autoFixture.Build<DraftApprenticeshipDetails>().Without(o=>o.Uln).Create(), Party.Provider);
                DraftApprenticeshipDetails = _autoFixture.Build<DraftApprenticeshipDetails>().Without(o => o.Uln).Create();
            }

            public DraftApprenticeshipUpdateTestFixture WithUlnUpdateOnly()
            {
                DraftApprenticeshipDetails = CreateUpdateFromOriginal();
                DraftApprenticeshipDetails.Uln = _autoFixture.Create<string>();
                return this;
            }

            public DraftApprenticeshipUpdateTestFixture WithReferenceUpdateOnly()
            {
                DraftApprenticeshipDetails = CreateUpdateFromOriginal();
                DraftApprenticeshipDetails.Reference = _autoFixture.Create<string>();
                return this;
            }

            public DraftApprenticeshipUpdateTestFixture WithTrainingCourseResetOnly()
            {
                DraftApprenticeshipDetails = CreateUpdateFromOriginal();
                DraftApprenticeshipDetails.TrainingProgramme = null;
                return this;
            }

            public DraftApprenticeshipUpdateTestFixture WithModifyingParty(Party party)
            {
                _modifyingParty = party;
                return this;
            }

            public DraftApprenticeshipUpdateTestFixture WithPriorApprovalByOtherParty()
            {
                switch (_modifyingParty.GetOtherParty())
                {
                    case Party.Employer:
                        _draftApprenticeship.AgreementStatus = AgreementStatus.EmployerAgreed;
                        break;
                    case Party.Provider:
                        _draftApprenticeship.AgreementStatus = AgreementStatus.ProviderAgreed;
                        break;
                    default:
                        break;
                }

                return this;
            }

            public CommitmentsV2.Models.DraftApprenticeship ApplyUpdate()
            {
                _result  = TestHelper.Clone(_draftApprenticeship);
                _result.Merge(DraftApprenticeshipDetails, _modifyingParty);
                return _result;
            }

            private DraftApprenticeshipDetails CreateUpdateFromOriginal()
            {
                return new DraftApprenticeshipDetails
                {
                    Uln = _draftApprenticeship.Uln,
                    FirstName = _draftApprenticeship.FirstName,
                    LastName = _draftApprenticeship.LastName,
                    Cost = (int?)_draftApprenticeship.Cost,
                    TrainingProgramme = new CommitmentsV2.Domain.Entities.TrainingProgramme(
                        _draftApprenticeship.CourseCode,
                        _draftApprenticeship.CourseName,
                        _draftApprenticeship.ProgrammeType.Value,
                        null,
                        null),
                    DateOfBirth = _draftApprenticeship.DateOfBirth,
                    StartDate = _draftApprenticeship.StartDate,
                    EndDate = _draftApprenticeship.EndDate,
                    Id = _draftApprenticeship.Id,
                    Reference = _modifyingParty == Party.Employer ? _draftApprenticeship.EmployerRef : _draftApprenticeship.ProviderRef
                };
            }
        }

    }
}
