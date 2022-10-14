using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Support.SubSite.Enums;
using SFA.DAS.Commitments.Support.SubSite.Mappers;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Orchestrators;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfProviderChain;
using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPriceEpisodes;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Orchestrators
{
    [TestFixture]
    [Parallelizable]
    public class WhenGettingCommitmentDetails
    {
        [Test, MoqAutoData]
        public void WhenGettingCommitmentDetails_InvalidCommitmentId_ShouldThrow(
           string hashedCommitmentId,
           string hashedAccountId,
           [Frozen] Mock<IEncodingService> encodingServiceMock,
           ApprenticeshipsOrchestrator sut
           )
        {
            // Arrange
            encodingServiceMock
                .Setup(o => o.Decode(hashedCommitmentId, EncodingType.CohortReference))
                .Throws(new Exception("Bad commitment ID"));

            // Act
            Assert.ThrowsAsync<Exception>(() => sut.GetCommitmentDetails(hashedCommitmentId, hashedAccountId));
        }
    }
}