using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Support.SubSite.Mappers;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Orchestrators.ApprenticeshipOrchestratorTests
{
    [TestFixture]
    internal class MapToOverlappingTrainingDateRequestMapperTests
    {
        MapToOverlappingTrainingDateRequestMapperTestsFixture fixture;
        public MapToOverlappingTrainingDateRequestMapperTests()
        {
            fixture = new MapToOverlappingTrainingDateRequestMapperTestsFixture();
        }

        [Test]
        public void When_There_Is_No_OverlappingTrainingDateRequest()
        {
            fixture.SetOverlappingDateRequest(null);
            fixture.Map();
            fixture.Verify_ViewModel_IsNull();
        }

        [Test]
        public void When_The_Status_Is_Not_Pending_On_OverlappingTrainingDateRequest()
        {
            fixture.SetStatus( OverlappingTrainingDateRequestStatus.Resolved);
            fixture.Map();
            fixture.Verify_ViewModel_IsNull();
        }

        [Test]
        public void When_The_Status_Is_Pending_On_OverlappingTrainingDateRequest()
        {
            fixture.SetStatus(OverlappingTrainingDateRequestStatus.Pending);
            fixture.Map();
            fixture.Verify_CreatedOn_IsMapped();
        }

        public class MapToOverlappingTrainingDateRequestMapperTestsFixture
        {
            GetOverlappingTrainingDateRequestQueryResult.OverlappingTrainingDateRequest _overlappingTrainingDateRequest;
            OverlappingTrainingDateRequestViewModel _overlappingTrainingDateRequestViewModel;
            private ApprenticeshipMapper _sut;

            public long ApprenticeshipId = 1;

            public MapToOverlappingTrainingDateRequestMapperTestsFixture()
            {
                var autoFixture = new Fixture();
                _overlappingTrainingDateRequest = autoFixture.Create<GetOverlappingTrainingDateRequestQueryResult.OverlappingTrainingDateRequest>();
                _sut = new ApprenticeshipMapper(Mock.Of<IEncodingService>());
            }

            public void Map()
            {
                _overlappingTrainingDateRequestViewModel = _sut.MapToOverlappingTrainingDateRequest(_overlappingTrainingDateRequest);
            }

            public void SetOverlappingDateRequest(GetOverlappingTrainingDateRequestQueryResult.OverlappingTrainingDateRequest request)
            {
                _overlappingTrainingDateRequest = request;
            }

            public void SetStatus(OverlappingTrainingDateRequestStatus status)
            {
                _overlappingTrainingDateRequest.Status = status;
            }

            public void Verify_ViewModel_IsNull()
            {
                Assert.IsNull(_overlappingTrainingDateRequestViewModel);
            }

            public void Verify_CreatedOn_IsMapped()
            {
                Assert.AreEqual(_overlappingTrainingDateRequest.CreatedOn, _overlappingTrainingDateRequestViewModel.CreatedOn);
            }
        }
    }
}
