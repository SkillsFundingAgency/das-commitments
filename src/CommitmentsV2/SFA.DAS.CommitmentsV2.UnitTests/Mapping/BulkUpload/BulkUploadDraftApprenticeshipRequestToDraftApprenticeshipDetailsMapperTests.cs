using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping.BulkUpload;
using SFA.DAS.Reservations.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private Mock<IReservationsApiClient> _reservationsApiClient;

        [SetUp]
        public async Task Arrange()
        {
            var autoFixture = new Fixture();
            _source = autoFixture.Create<BulkUploadAddDraftApprenticeshipsCommand>();
            _source.BulkUploadDraftApprenticeships.ForEach(x => { x.CourseCode = "2"; x.ReservationId = null; });

            _trainingProgramme = new TrainingProgramme("2", "TrainingProgramme", "1.0", "1.1", Types.ProgrammeType.Standard, new DateTime(2050, 1, 1), new DateTime(2060, 1, 1), new System.Collections.Generic.List<CommitmentsV2.Models.IFundingPeriod>());
            _trainingLookup = new Mock<ITrainingProgrammeLookup>();
            _trainingLookup.Setup(s => s.GetCalculatedTrainingProgrammeVersion(It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(() => _trainingProgramme);

            _reservationsApiClient = new Mock<IReservationsApiClient>();
            SetupReservations(_source);
            _mapper = new BulkUploadAddDraftApprenticeshipRequestToDraftApprenticeshipDetailsMapper(_trainingLookup.Object, _reservationsApiClient.Object);
            _result = await _mapper.Map(TestHelper.Clone(_source));
        }

        void SetupReservations(BulkUploadAddDraftApprenticeshipsCommand command)
        {
            Guid[] GetGuids(int count)
            {
                var guids = new Guid[count];
                for (var i = 0; i < count; i++)
                {
                    guids[i] = Guid.NewGuid();
                }

                return guids;
            }

            var legalEntities = command.BulkUploadDraftApprenticeships.GroupBy(x => x.LegalEntityId).Select(y => new { LegalEntityId = y.Key, Count = y.Count() });
            foreach (var lg in legalEntities)
            {
                _reservationsApiClient.Setup(x => x.BulkCreateReservations(lg.LegalEntityId, It.IsAny<BulkCreateReservationsRequest>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => new BulkCreateReservationsResult(GetGuids(lg.Count)));
            }
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
        public void ReservationIdIsMappedCorrectly()
        {
            Assert.IsTrue(_result.All(x => x.ReservationId != null));
        }


        [Test]
        public void ReservationIdApiServiceIsCalledOnceForEachLegalEntity()
        {
            var legalEntities = _source.BulkUploadDraftApprenticeships.GroupBy(x => x.LegalEntityId).Select(y => new { LegalEntityId = y.Key, Count = y.Count() });
            foreach (var legalEntity in legalEntities)
            {
                _reservationsApiClient.Verify(x => x.BulkCreateReservations(legalEntity.LegalEntityId, It.Is<BulkCreateReservationsRequest>(x => x.Count == legalEntity.Count && x.TransferSenderId == null), It.IsAny<CancellationToken>()), Times.Once);
            }
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
}
