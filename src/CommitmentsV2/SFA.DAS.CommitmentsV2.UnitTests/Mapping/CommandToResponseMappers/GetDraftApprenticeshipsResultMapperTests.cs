﻿using System.Threading.Tasks;
using AutoFixture;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Mapping.CommandToResponseMappers;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.CommandToResponseMappers
{
    public class GetDraftApprenticeshipsResultMapperTests
    {
        private GetDraftApprenticeshipsResultMapper _mapper;
        private GetDraftApprenticeshipsResult _source;
        private GetDraftApprenticeshipsResponse _result;

        [SetUp]
        public async Task Arrange()
        {
            var autoFixture = new Fixture();
            _source = autoFixture.Create<GetDraftApprenticeshipsResult>();

            _mapper = new GetDraftApprenticeshipsResultMapper();

            _result = await _mapper.Map(TestHelper.Clone(_source));
        }

        [Test]
        public void DraftApprenticeshipsAreMappedCorrectly()
        {
            var compare = new CompareLogic();
            var compareResult = compare.Compare(_source.DraftApprenticeships, _result.DraftApprenticeships);
            Assert.IsTrue(compareResult.AreEqual);
        }
    }
}
