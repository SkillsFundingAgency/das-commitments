using System;
using System.Collections.Generic;
using System.Linq;

using Castle.Components.DictionaryAdapter;
using FluentAssertions;
using NUnit.Framework;

using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.DataLock.Types;
using SFA.DAS.Commitments.Application.Services;

namespace SFA.DAS.Commitments.Application.UnitTests.Service.Facets
{
    [TestFixture]
    public class WhenExtractingRecordStatuses
    {
        private List<Apprenticeship> _data;

        private ApprenticeshipSearchQuery _userQuery;

        private FacetMapper _sut;

        [SetUp]
        public void SetUp()
        {
            _data = new List<Apprenticeship>
                        {
                            new Apprenticeship
                                {
                                    FirstName = "Not started",
                                    PaymentStatus = PaymentStatus.Active,
                                    StartDate = DateTime.Now.AddDays(-30),
                                    DataLockCourse = true
                                }
                        };

            _userQuery = new ApprenticeshipSearchQuery();
            _sut = new FacetMapper();
        }

        [TestCase(Originator.Provider)]
        [TestCase(Originator.Employer)]
        public void ShouldMapChangesPendingIfCallerHasUpdated(Originator caller)
        {
            _data.Add(new Apprenticeship { PendingUpdateOriginator = caller });
            _data.Add(new Apprenticeship { PendingUpdateOriginator = caller });

            var result = _sut.BuildFacets(_data, _userQuery, caller);

            AssertStatuses(result, RecordStatus.ChangesPending, 1);
            AssertStatuses(result, RecordStatus.ChangesForReview, 0);
        }

        [TestCase(Originator.Provider, Originator.Employer)]
        [TestCase(Originator.Employer, Originator.Provider)]
        public void ShouldMapToChangesToReviewHasUpdatesFromOtherPart(Originator caller, Originator updater)
        {
            _data.Add(new Apprenticeship { PendingUpdateOriginator = updater });
            _data.Add(new Apprenticeship { PendingUpdateOriginator = updater });

            var result = _sut.BuildFacets(_data, _userQuery, caller);

            AssertStatuses(result, RecordStatus.ChangesPending, 0);
            AssertStatuses(result, RecordStatus.ChangesForReview, 1);
        }

        [TestCase(Originator.Provider)]
        [TestCase(Originator.Employer)]
        public void ShouldHaveBothPendingAndForReview(Originator caller)
        {
            _data.Add(new Apprenticeship { PendingUpdateOriginator = Originator.Provider });
            _data.Add(new Apprenticeship { PendingUpdateOriginator = Originator.Provider });
            _data.Add(new Apprenticeship { PendingUpdateOriginator = Originator.Employer });
            _data.Add(new Apprenticeship { PendingUpdateOriginator = Originator.Employer });

            var result = _sut.BuildFacets(_data, _userQuery, caller);

            AssertStatuses(result, RecordStatus.ChangesPending, 1);
            AssertStatuses(result, RecordStatus.ChangesForReview, 1);
        }

        [TestCase(Originator.Provider)]
        [TestCase(Originator.Employer)]
        public void ShouldHaveDataLockRestartFacetAndPendingWhenUpdateFromTheSameAsCaller(Originator caller)
        {
            _data.Add(new Apprenticeship { PendingUpdateOriginator = caller, DataLockCourseTriaged = true});

            var result = _sut.BuildFacets(_data, _userQuery, caller);

            AssertStatuses(result, RecordStatus.ChangesPending, 1);
            AssertStatuses(result, RecordStatus.ChangeRequested, 1);
        }

        [Test]
        public void ShouldHaveDataLockRestartFacetAndChangeForReviewForEmployer()
        {
            _data.Add(new Apprenticeship { PendingUpdateOriginator = Originator.Provider, DataLockCourseTriaged = true });

            var result = _sut.BuildFacets(_data, _userQuery, Originator.Employer);

            AssertStatuses(result, RecordStatus.ChangesForReview, 1);
            AssertStatuses(result, RecordStatus.ChangeRequested, 1);
        }

        [TestCase(Originator.Provider, Originator.Provider)]
        [TestCase(Originator.Employer, Originator.Employer)]
        public void ShouldHaveDataLockRestartAndPendingUpdateAnd_AllShouldBeSelected(Originator updateFrom, Originator caller)
        {
            _data.Add(new Apprenticeship { PendingUpdateOriginator = updateFrom, DataLockCourseTriaged = true });
            _userQuery.RecordStatuses = new EditableList<RecordStatus> { RecordStatus.ChangeRequested, RecordStatus.ChangesForReview, RecordStatus.ChangesPending  };
            var result = _sut.BuildFacets(_data, _userQuery, caller);

            result.RecordStatuses.Any(m => m.Selected && m.Data == RecordStatus.ChangesPending).Should().BeTrue();
            result.RecordStatuses.Any(m => m.Selected && m.Data == RecordStatus.ChangeRequested).Should().BeTrue();

            result.RecordStatuses.Any(m => m.Selected && m.Data == RecordStatus.ChangesForReview).Should().BeFalse();

            AssertStatuses(result, RecordStatus.ChangesPending, 1);
            AssertStatuses(result, RecordStatus.ChangeRequested, 1);
        }

        [TestCase(Originator.Provider, Originator.Employer)]
        [TestCase(Originator.Employer, Originator.Provider)]
        public void ShouldHaveDataLockRestartAndFacetChangesForReview_AllShouldBeSelected(Originator updateFrom, Originator caller)
        {
            _data.Add(new Apprenticeship { PendingUpdateOriginator = updateFrom, DataLockCourseTriaged = true});
            _userQuery.RecordStatuses = new EditableList<RecordStatus> { RecordStatus.ChangeRequested, RecordStatus.ChangesForReview, RecordStatus.ChangesPending };
            var result = _sut.BuildFacets(_data, _userQuery, caller);

            result.RecordStatuses.Any(m => m.Selected && m.Data == RecordStatus.ChangeRequested).Should().BeTrue();
            result.RecordStatuses.Any(m => m.Selected && m.Data == RecordStatus.ChangesForReview).Should().BeTrue();

            result.RecordStatuses.Any(m => m.Selected && m.Data == RecordStatus.ChangesPending).Should().BeFalse();

            AssertStatuses(result, RecordStatus.ChangesForReview, 1);
            AssertStatuses(result, RecordStatus.ChangeRequested, 1);
        }

        private void AssertStatuses(Api.Types.Apprenticeship.Facets result, RecordStatus status, int i)
        {
            result.RecordStatuses.Count(m => m.Data == status).Should().Be(i);
        }
    }
}
