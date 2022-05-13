using AutoFixture;
using AutoFixture.Kernel;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping.BulkUpload;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Reservations.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.BulkUpload
{
    [TestFixture]
    public class BulkUploadDraftApprenticeshipRequestToDraftApprenticeshipDetailsMapperTests
    {
        private BulkUploadAddDraftApprenticeshipRequestToDraftApprenticeshipDetailsMapper _mapper;
        private BulkUploadAddDraftApprenticeshipsCommand _source;
        private List<DraftApprenticeshipDetails> _result;
        private Mock<ITrainingProgrammeLookup> _trainingLookup;
        private TrainingProgramme _trainingProgramme;
        public List<Account> SeedAccounts { get; set; }

        [SetUp]
        public async Task Arrange()
        {
            var autoFixture = new Fixture();
            SeedAccounts = new List<Account>();
            autoFixture.Customizations.Add(new BulkUploadAddDraftApprenticeshipRequestSpecimenBuilder("PUB456", 1));
            _source = autoFixture.Create<BulkUploadAddDraftApprenticeshipsCommand>();
            _source.UserInfo.UserId = Guid.NewGuid().ToString();
            _source.BulkUploadDraftApprenticeships.ForEach(x => { x.CourseCode = "2"; x.ReservationId = null; });

            _trainingProgramme = new TrainingProgramme("2", "TrainingProgramme", "1.0", "1.1", Types.ProgrammeType.Standard, new DateTime(2050, 1, 1), new DateTime(2060, 1, 1), new System.Collections.Generic.List<CommitmentsV2.Models.IFundingPeriod>());
            _trainingLookup = new Mock<ITrainingProgrammeLookup>();
            _trainingLookup.Setup(s => s.GetCalculatedTrainingProgrammeVersion(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(() => _trainingProgramme);

            _mapper = new BulkUploadAddDraftApprenticeshipRequestToDraftApprenticeshipDetailsMapper(_trainingLookup.Object);
            _result = await _mapper.Map(TestHelper.Clone(_source));
        }

        [Test]
        public void FirstNameIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.First(y => y.Uln == source.Uln);
                Assert.AreEqual(source.FirstName, result.FirstName);
            });
        }

        [Test]
        public void LastNameIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.First(y => y.Uln == source.Uln);
                Assert.AreEqual(source.LastName, result.LastName);
            });
        }

        [Test]
        public void EmailIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.First(y => y.Uln == source.Uln);
                Assert.AreEqual(source.Email, result.Email);
            });
        }

        [Test]
        public void UlnIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.First(y => y.Uln == source.Uln);
                Assert.AreEqual(source.Uln, result.Uln);
            });
        }

        [Test]
        public void CostIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.First(y => y.Uln == source.Uln);
                Assert.AreEqual(source.Cost, result.Cost);
            });
        }

        [Test]
        public void StartDateIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.First(y => y.Uln == source.Uln);
                Assert.AreEqual(source.StartDate, result.StartDate);
            });
        }

        [Test]
        public void EndDateIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.First(y => y.Uln == source.Uln);
                Assert.AreEqual(source.EndDate, result.EndDate);
            });
        }

        [Test]
        public void DateOfBirthIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.First(y => y.Uln == source.Uln);
                Assert.AreEqual(source.DateOfBirth, result.DateOfBirth);
            });
        }

        [Test]
        public void ReferenceIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.First(y => y.Uln == source.Uln);
                Assert.AreEqual(source.ProviderRef, result.Reference);
            });
        }

        [Test]
        public void TrainingCourseVersionIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.First(y => y.Uln == source.Uln);
                Assert.AreEqual(_trainingProgramme.Version, result.TrainingCourseVersion);
            });
        }

        [Test]
        public void StandardUIdIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.First(y => y.Uln == source.Uln);
                Assert.AreEqual(_trainingProgramme.StandardUId, result.StandardUId);
            });
        }

        [Test]
        public void TrainingProgrammeIsMappedCorrectly()
        {
            foreach (var rs in _result)
            {
                Assert.AreEqual(_trainingProgramme.CourseCode, rs.TrainingProgramme.CourseCode);
                Assert.AreEqual(_trainingProgramme.EffectiveFrom, rs.TrainingProgramme.EffectiveFrom);
                Assert.AreEqual(_trainingProgramme.EffectiveTo, rs.TrainingProgramme.EffectiveTo);
            }
        }
    }

    public class BulkUploadAddDraftApprenticeshipRequestSpecimenBuilder : ISpecimenBuilder
    {
        public long? LegalEntityId { get; set; }
        public string AgreementId { get; set; }
        public BulkUploadAddDraftApprenticeshipRequestSpecimenBuilder(string agreementId, long legalEntityId)
        {
            AgreementId = agreementId;
            LegalEntityId = legalEntityId;
        }

        public BulkUploadAddDraftApprenticeshipRequestSpecimenBuilder() { }

        public object Create(object request, ISpecimenContext context)
        {
            if (request is Type type && type == typeof(BulkUploadAddDraftApprenticeshipRequest))
            {
                var fixture = new Fixture();
                var startDate = fixture.Create<DateTime>();
                var endDate = fixture.Create<DateTime>();
                var dob = fixture.Create<DateTime>();
                var buildDraftApprenticeshipRequest = fixture.Build<BulkUploadAddDraftApprenticeshipRequest>()
                    .With(x => x.StartDateAsString, startDate.ToString("yyyy-MM-dd"))
                    .With(x => x.EndDateAsString, endDate.ToString("yyyy-MM"))
                    .With(x => x.DateOfBirthAsString, dob.ToString("yyyy-MM-dd"))
                    .With(x => x.CostAsString, "1000");

                if (LegalEntityId.HasValue)
                {
                    buildDraftApprenticeshipRequest = buildDraftApprenticeshipRequest
                           .With(x => x.LegalEntityId, LegalEntityId)
                           .With(x => x.AgreementId, AgreementId);
                }

               return buildDraftApprenticeshipRequest.Create();
            }

            return new NoSpecimen();
        }
    }
}
