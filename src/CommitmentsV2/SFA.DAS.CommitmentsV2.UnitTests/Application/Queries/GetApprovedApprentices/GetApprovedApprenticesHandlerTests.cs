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
using SFA.DAS.CommitmentsV2.Mapping;
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
            List<Apprenticeship> approvedApprenticeships,
            ApprenticeshipDetails apprenticeshipDetails,
            [Frozen] Mock<IProviderCommitmentsDbContext> mockContext,
            [Frozen] Mock<IMapper<Apprenticeship, ApprenticeshipDetails>> mockMapper,
            GetApprovedApprenticesHandler handler)
        {
            approvedApprenticeships[0].Cohort.ProviderId = request.ProviderId;
            approvedApprenticeships[1].Cohort.ProviderId = request.ProviderId;
            mockContext
                .Setup(context => context.Apprenticeships)
                .ReturnsDbSet(approvedApprenticeships);
            mockMapper
                .Setup(mapper => mapper.Map(It.IsIn(approvedApprenticeships
                    .Where(apprenticeship => apprenticeship.Cohort.ProviderId == request.ProviderId))))
                .ReturnsAsync(apprenticeshipDetails);

            var result = await handler.Handle(request, CancellationToken.None);

            result.Apprenticeships.Count()
                .Should().Be(approvedApprenticeships
                    .Count(apprenticeship => apprenticeship.Cohort.ProviderId == request.ProviderId));
            result.Apprenticeships.Should().AllBeEquivalentTo(apprenticeshipDetails);
        }
    }
}