using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Kernel;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeshipUpdate
{
    [TestFixture]
    public class GetApprenticeshipUpdateHandlerTests
    {
        private GetApprenticeshipUpdateHandlerTests_fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetApprenticeshipUpdateHandlerTests_fixture();
        }

        [Test]
        public async Task Handle_ThenShouldReturnResultWithValues()
        {
            _fixture.SeedData();
             await _fixture.Handle();
            _fixture.VerifyResultMapping(3);
        }

        [Test]
        public async Task Handle_ThenShouldReturnAnEmptyArray()
        {
            _fixture.SeedData().WithNoMatchingApprenticeshipUpdates();
            var result = await _fixture.Handle();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ApprenticeshipUpdates, Is.Empty);
        }

        [Test]
        public async Task Handle_WhenStatusNotFound_ThenShouldReturnAnEmptyArray()
        {
            _fixture.SeedData().WithNoMatchingApprenticeshipUpdatesForStatus();
            var result = await _fixture.Handle();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ApprenticeshipUpdates, Is.Empty);
        }

        [Test]
        public async Task Handle_ThenShouldReturnArrayWithMatchedStatus()
        {
            _fixture.SeedData().SetStatusRejectedForTheFirstApprenticeshipUpdate().WithRejectedStatus();
            await _fixture.Handle();
            _fixture.VerifyResultMapping(1);
        }

        [Test]
        public async Task Handle_ThenShouldReturnResultWithTheFirstApprenticeshipUpdate()
        {
            _fixture.SeedData(2);
            await _fixture.Handle();
            _fixture.VerifyResultMapping(3);
        }

        [Test]
        public async Task Handle_WhenStatusNull_ThenShouldReturnArrayWithMatchedApprenticeshipId()
        {
            _fixture.SeedData().SetStatusRejectedForTheFirstApprenticeshipUpdate().WithNullStatus();
            await _fixture.Handle();
            _fixture.VerifyResultMapping(3);
        }

        public class GetApprenticeshipUpdateHandlerTests_fixture
        {
            private readonly GetApprenticeshipUpdateQueryHandler _handler;
            private readonly Mock<ProviderCommitmentsDbContext> _db;
            private GetApprenticeshipUpdateQuery _request;
            private GetApprenticeshipUpdateQueryResult _result;
            private readonly Fixture _autofixture;
            private List<ApprenticeshipUpdate> _apprenticeshipUpdate;
            private readonly long _apprenticeshipId;

            public GetApprenticeshipUpdateHandlerTests_fixture()
            {
                _autofixture = new Fixture();

                _apprenticeshipId = 1;
                _request = new GetApprenticeshipUpdateQuery(_apprenticeshipId, Types.ApprenticeshipUpdateStatus.Pending);

                _db = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                        .UseInMemoryDatabase(Guid.NewGuid().ToString()).EnableSensitiveDataLogging().Options)
                    {CallBase = true};

                _handler = new GetApprenticeshipUpdateQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db.Object));
            }

            public async Task<GetApprenticeshipUpdateQueryResult> Handle()
            {
                _result = await _handler.Handle(_request, new CancellationToken());
                return _result;
            }

            public GetApprenticeshipUpdateHandlerTests_fixture SeedData(short count = 1)
            {
                _autofixture.Customizations.Add(new ApprenticeshipUpdateSpecimenBuilder());
                _apprenticeshipUpdate = _autofixture.Create<List<ApprenticeshipUpdate>>();
                _apprenticeshipUpdate.ForEach(z => { z.ApprenticeshipId = _apprenticeshipId; z.Status = Types.ApprenticeshipUpdateStatus.Pending;  z.Apprenticeship.Id = _apprenticeshipId; });
             
                _db
                    .Setup(context => context.ApprenticeshipUpdates)
                    .ReturnsDbSet(_apprenticeshipUpdate);

                var additionalRecord = _autofixture.Create<List<ApprenticeshipUpdate>>();
                additionalRecord.ForEach(z => { z.ApprenticeshipId = ++count; z.Apprenticeship.Id = z.ApprenticeshipId; }) ;
                _apprenticeshipUpdate.AddRange(additionalRecord);

                return this;
            }

            public GetApprenticeshipUpdateHandlerTests_fixture WithNoMatchingApprenticeshipUpdates()
            {
                _request = new GetApprenticeshipUpdateQuery(_apprenticeshipId + 100, Types.ApprenticeshipUpdateStatus.Pending);
                return this;
            }

            public GetApprenticeshipUpdateHandlerTests_fixture WithRejectedStatus()
            {
                _request = new GetApprenticeshipUpdateQuery(_apprenticeshipId, Types.ApprenticeshipUpdateStatus.Rejected);
                return this;
            }

            internal GetApprenticeshipUpdateHandlerTests_fixture WithNullStatus()
            {
                _request = new GetApprenticeshipUpdateQuery(_apprenticeshipId, null);
                return this;
            }

            public GetApprenticeshipUpdateHandlerTests_fixture WithNoMatchingApprenticeshipUpdatesForStatus()
            {
                _request = new GetApprenticeshipUpdateQuery(_apprenticeshipId, Types.ApprenticeshipUpdateStatus.Approved);
                return this;
            }

            public GetApprenticeshipUpdateHandlerTests_fixture SetStatusRejectedForTheFirstApprenticeshipUpdate()
            {
                var apprenticeship = _db.Object.ApprenticeshipUpdates.First(x => x.ApprenticeshipId == _apprenticeshipId && x.Status == Types.ApprenticeshipUpdateStatus.Pending);
                apprenticeship.Status = Types.ApprenticeshipUpdateStatus.Rejected;
                return this;
            }

            public void VerifyResultMapping(int resultCount)
            {
                Assert.That(_result.ApprenticeshipUpdates, Has.Count.EqualTo(resultCount));

                foreach (var result in _result.ApprenticeshipUpdates)
                {
                    AssertEquality(_apprenticeshipUpdate.Single(x => x.Id == result.Id), result);
                }
            }
        }

        private static void AssertEquality(ApprenticeshipUpdate source, GetApprenticeshipUpdateQueryResult.ApprenticeshipUpdate result)
        {
            Assert.Multiple(() =>
            {
                Assert.That(result.Id, Is.EqualTo(source.Id));

                Assert.That(result.ApprenticeshipId, Is.EqualTo(source.ApprenticeshipId));
                Assert.That(result.Originator, Is.EqualTo(source.Originator));
                Assert.That(result.FirstName, Is.EqualTo(source.FirstName));
                Assert.That(result.LastName, Is.EqualTo(source.LastName));
                Assert.That(result.DeliveryModel, Is.EqualTo(source.DeliveryModel));
                Assert.That(result.EmploymentEndDate, Is.EqualTo(source.EmploymentEndDate));
                Assert.That(result.EmploymentPrice, Is.EqualTo(source.EmploymentPrice));
                Assert.That(result.TrainingType, Is.EqualTo(source.TrainingType));
                Assert.That(result.TrainingCode, Is.EqualTo(source.TrainingCode));
                Assert.That(result.TrainingName, Is.EqualTo(source.TrainingName));
                Assert.That(result.TrainingCourseVersion, Is.EqualTo(source.TrainingCourseVersion));
                Assert.That(result.TrainingCourseOption, Is.EqualTo(source.TrainingCourseOption));
                Assert.That(result.Cost, Is.EqualTo(source.Cost));
                Assert.That(result.StartDate, Is.EqualTo(source.StartDate));
                Assert.That(result.EndDate, Is.EqualTo(source.EndDate));
                Assert.That(result.DateOfBirth, Is.EqualTo(source.DateOfBirth));
                Assert.That(result.Email, Is.EqualTo(source.Email));
            });
        }
    }

    public class ApprenticeshipUpdateSpecimenBuilder :
    ISpecimenBuilder
    {
        public object Create(object request,
            ISpecimenContext context)
        {
            var pi = request as Type;
            
            if (pi == null)
            {
                return new NoSpecimen();
            }
            if (pi == typeof(ApprenticeshipBase)
                || pi.Name == "Apprenticeship")
            {
                return new Apprenticeship();
            }

            if (IsListOrCollection(pi) && pi.GetGenericArguments().Single() == typeof(DataLockStatus)
               || pi.Name == "DataLockStatus")
            {
                return new List<DataLockStatus>();
            }

            return new NoSpecimen();
        }

        private static bool IsListOrCollection(Type type)
        {
            return type != typeof(string) && (type.GetInterface(nameof(System.Collections.IEnumerable)) != null || type.GetInterface(nameof(System.Collections.ICollection)) != null);
        }
    }
}
