//todo: These tests test QueryStringHelper.
// The actual version of QueryStringHelper used by the client code is internal in das-shared-packages/SFA.DAS.Http,
// so these tests test against a c&p version of QueryStringHelper in SFA.DAS.Commitments.Api.Client that's been changed to public!
// The test coverage in SFA.DAS.Http is poor - it only (indirectly) tests 1 simple scenario, and the coverage here is much better.
// However, it's confusing having a version of QueryStringHelper in ...Api.Client.
// The code isn't actually used and might get out-of-sync with the real code, so these tests have no relation to real-world behaviour
// and someone might assume the local copy of the code is actually used (as I did) and fix issues/refactor the non-used code.
// To sort this out we could... (options off top of head)
// Improve test coverage in the shared package (probably the best option)
// Test indirectly through the actual client in this solution
// Test directly in this solution by using techniques to test non-visible code

//using System.Collections.Generic;

//using FluentAssertions;
//using NUnit.Framework;

//using SFA.DAS.Commitments.Api.Types.Apprenticeship;
//using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;

//namespace SFA.DAS.Commitments.Api.Client.UnitTests
//{

//    [TestFixture]
//    public class HelperTest
//    {
//        private QueryStringHelper _sut;

//        [SetUp]
//        public void SetUp()
//        {
//            _sut = new QueryStringHelper();
//        }

//        [Test]
//        public void TestDefaultQuery()
//        {
//            var query = new ApprenticeshipSearchQuery();
//            var queryString = _sut.GetQueryString(query);

//            queryString.Should().Be("?PageNumber=1&PageSize=25");
//        }

//        [Test]
//        public void TestEmptyQueryNull()
//        {
//            var queryString = _sut.GetQueryString(null);

//            queryString.Should().Be("");
//        }

//        [Test]
//        public void TestApprenticeshipStatus()
//        {
//            var query = new ApprenticeshipSearchQuery
//                            {
//                                ApprenticeshipStatuses = new List<ApprenticeshipStatus> { ApprenticeshipStatus.Finished, ApprenticeshipStatus.Paused }
//                            };

//            var queryString = _sut.GetQueryString(query);

//            queryString.Should().Be("?ApprenticeshipStatuses=Finished&ApprenticeshipStatuses=Paused&PageNumber=1&PageSize=25");
//        }

//        [Test]
//        public void TestRecordStatuses()
//        {
//            var query = new ApprenticeshipSearchQuery
//            {
//                RecordStatuses = new List<RecordStatus> { RecordStatus.ChangeRequested, RecordStatus.ChangesPending }
//            };

//            var queryString = _sut.GetQueryString(query);

//            queryString.Should().Be("?RecordStatuses=ChangeRequested&RecordStatuses=ChangesPending&PageNumber=1&PageSize=25");
//        }

//        [Test]
//        public void TestTransferFunder()
//        {
//            var query = new ApprenticeshipSearchQuery
//            {
//                TransferFunded = true
//            };

//            var queryString = _sut.GetQueryString(query);

//            queryString.Should().Be("?TransferFunded=True&PageNumber=1&PageSize=25");
//        }

//        [Test]
//        public void ShouldHandleAllEmptyLists()
//        {
//            var query = new ApprenticeshipSearchQuery
//            {
//                ApprenticeshipStatuses = new List<ApprenticeshipStatus>(),
//                RecordStatuses = new List<RecordStatus>(),
//                EmployerOrganisationIds = new List<string>(),
//                TrainingCourses = new List<string>(),
//                TrainingProviderIds = null
//            };

//            var queryString = _sut.GetQueryString(query);

//            queryString.Should().Be("?PageNumber=1&PageSize=25");
//        }
//    }
//}