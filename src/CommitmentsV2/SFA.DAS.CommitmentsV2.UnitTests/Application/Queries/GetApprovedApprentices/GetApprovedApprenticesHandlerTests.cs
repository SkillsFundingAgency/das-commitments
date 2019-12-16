using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprentices;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprovedApprentices
{
    public class GetApprovedApprenticesHandlerTests
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_Returns_Approved_Apprentices(
            GetApprovedApprenticesRequest request,
            List<CommitmentsV2.Models.ApprovedApprenticeship> approvedApprenticeships,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            GetApprovedApprenticesHandler handler)
        {
            approvedApprenticeships[0].ProviderRef = request.ProviderId.ToString();
            approvedApprenticeships[1].ProviderRef = request.ProviderId.ToString();

            mockContext
                .Setup(context => context.ApprovedApprenticeships)
                .ReturnsDbSet(approvedApprenticeships);

            var result = await handler.Handle(request, CancellationToken.None);

            result.Apprenticeships.Should().BeEquivalentTo(approvedApprenticeships
                .Where(apprenticeship => apprenticeship.ProviderRef == request.ProviderId.ToString())
                .Select(apprenticeship => (ApprenticeshipDetails)apprenticeship));
        }
    }
}