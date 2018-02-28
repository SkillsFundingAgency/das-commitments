using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup
{
    public class TestData
    {
        public List<DbSetupApprenticeship> GenerateApprenticeships()
        {
            var fixture = new Fixture();//.Customize(new IntegrationTestCustomisation());
            var apprenticeships = fixture.CreateMany<DbSetupApprenticeship>(2).ToList();
            return apprenticeships;
        }
    }
}
