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

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetPendingApprenticeChanges
{
    [TestFixture]
    public class GetPendingApprenticeChangesQueryHandlerTests
    {
        private GetPendingApprenticeChangesQueryHandlerTests_Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetPendingApprenticeChangesQueryHandlerTests_Fixture();
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
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.ApprenticeshipUpdates.Count);
        }

        public class GetPendingApprenticeChangesQueryHandlerTests_Fixture
        {
            private readonly GetApprenticeshipUpdateQueryHandler _handler;
            private readonly Mock<ProviderCommitmentsDbContext> _db;
            private GetApprenticeshipUpdateQuery _request;
            private GetApprenticeshipUpdateQueryResult _result;
            private readonly Fixture _autofixture;
            private List<ApprenticeshipUpdate> _apprenticeshipUpdate;
            private readonly long _apprenticeshipId;

            public GetPendingApprenticeChangesQueryHandlerTests_Fixture()
            {
                _autofixture = new Fixture();

                _apprenticeshipId = 1;
                _request = new GetApprenticeshipUpdateQuery(_apprenticeshipId, Types.ApprenticeshipUpdateStatus.Pending);

                _db = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                        .UseInMemoryDatabase(Guid.NewGuid().ToString()).EnableSensitiveDataLogging().Options)
                { CallBase = true };

                _handler = new GetApprenticeshipUpdateQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db.Object));
            }

            public async Task<GetApprenticeshipUpdateQueryResult> Handle()
            {
                _result = await _handler.Handle(_request, new CancellationToken());
                return _result;
            }

            public GetPendingApprenticeChangesQueryHandlerTests_Fixture SeedData(short count = 1)
            {
                _autofixture.Customizations.Add(new ApprenticeshipUpdateSpecimenBuilder());
                _apprenticeshipUpdate = _autofixture.Create<List<ApprenticeshipUpdate>>();
                _apprenticeshipUpdate.ForEach(z => { z.ApprenticeshipId = _apprenticeshipId; z.Status = Types.ApprenticeshipUpdateStatus.Pending; z.Apprenticeship.Id = _apprenticeshipId; });

                _db
                    .Setup(context => context.ApprenticeshipUpdates)
                    .ReturnsDbSet(_apprenticeshipUpdate);

                var additionalRecord = _autofixture.Create<List<ApprenticeshipUpdate>>();
                additionalRecord.ForEach(z => { z.ApprenticeshipId = ++count; z.Apprenticeship.Id = z.ApprenticeshipId; });
                _apprenticeshipUpdate.AddRange(additionalRecord);

                return this;
            }

            public GetPendingApprenticeChangesQueryHandlerTests_Fixture WithNoMatchingApprenticeshipUpdates()
            {
                _request = new GetApprenticeshipUpdateQuery(_apprenticeshipId + 100, Types.ApprenticeshipUpdateStatus.Pending);
                return this;
            }

            public void VerifyResultMapping(int resultCount)
            {
                Assert.AreEqual(resultCount, _result.ApprenticeshipUpdates.Count);

                foreach (var result in _result.ApprenticeshipUpdates)
                {
                    AssertEquality(_apprenticeshipUpdate.Single(x => x.Id == result.Id), result);
                }
            }

            private static void AssertEquality(ApprenticeshipUpdate source, GetApprenticeshipUpdateQueryResult.ApprenticeshipUpdate result)
            {
                Assert.AreEqual(source.Id, result.Id);
                Assert.AreEqual(source.ApprenticeshipId, result.ApprenticeshipId);
                Assert.AreEqual(source.Originator, result.Originator);
                Assert.AreEqual(source.FirstName, result.FirstName);
                Assert.AreEqual(source.LastName, result.LastName);
                Assert.AreEqual(source.DeliveryModel, result.DeliveryModel);
                Assert.AreEqual(source.EmploymentEndDate, result.EmploymentEndDate);
                Assert.AreEqual(source.EmploymentPrice, result.EmploymentPrice);
                Assert.AreEqual(source.TrainingType, result.TrainingType);
                Assert.AreEqual(source.TrainingCode, result.TrainingCode);
                Assert.AreEqual(source.TrainingName, result.TrainingName);
                Assert.AreEqual(source.TrainingCourseVersion, result.TrainingCourseVersion);
                Assert.AreEqual(source.TrainingCourseOption, result.TrainingCourseOption);
                Assert.AreEqual(source.Cost, result.Cost);
                Assert.AreEqual(source.StartDate, result.StartDate);
                Assert.AreEqual(source.EndDate, result.EndDate);
                Assert.AreEqual(source.DateOfBirth, result.DateOfBirth);
                Assert.AreEqual(source.Email, result.Email);
            }
        }
    }

    public class ApprenticeshipUpdateSpecimenBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
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
