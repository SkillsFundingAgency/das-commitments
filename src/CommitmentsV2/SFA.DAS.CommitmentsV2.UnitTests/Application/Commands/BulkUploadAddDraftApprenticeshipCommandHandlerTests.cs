﻿using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Reservations.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    public class BulkUploadAddDraftApprenticeshipCommandHandlerTests
    {
        [Test]
        public async Task DraftApprenticeshipDetailMapperIsCalled()
        {
            var fixture = new BulkUploadAddDraftApprenticeshipCommandHandlerTestsixture();
            await fixture.Handle();

            fixture.VerifyMapperIsCalled();
        }

        [Test]
        public async Task DraftApprenticeshipDetailAreAdded()
        {
            var fixture = new BulkUploadAddDraftApprenticeshipCommandHandlerTestsixture();
            await fixture.Handle();

            fixture.VerifyDraftApprenticeshipsAreAdded();
        }
    }

    public class BulkUploadAddDraftApprenticeshipCommandHandlerTestsixture
    {
        public Fixture AutoFixture { get; set; }
        public Mock<ICohortDomainService> CohortDomainService { get; set; }
        public IRequestHandler<BulkUploadAddDraftApprenticeshipsCommand> Handler { get; set; }
        public BulkUploadAddDraftApprenticeshipsCommand Command { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public Mock<IReservationsApiClient> ReservationApiClient { get; set; }
        public Mock<IModelMapper> ModelMapper { get; }
        public List<DraftApprenticeshipDetails> DraftApprenticeshipDetails { get; set; }

        public BulkUploadAddDraftApprenticeshipCommandHandlerTestsixture()
        {
            AutoFixture = new Fixture();
            CohortDomainService = new Mock<ICohortDomainService>();
            ReservationApiClient = new Mock<IReservationsApiClient>();
            ModelMapper = new Mock<IModelMapper>();
            Command = AutoFixture.Create<BulkUploadAddDraftApprenticeshipsCommand>();

            DraftApprenticeshipDetails = AutoFixture.Create<List<DraftApprenticeshipDetails>>();
            DraftApprenticeshipDetails = DraftApprenticeshipDetails.Zip(Command.BulkUploadDraftApprenticeships, (x, y) => { x.Uln = y.Uln; return x; }).ToList();

            ModelMapper.Setup(x => x.Map<List<DraftApprenticeshipDetails>>(It.IsAny<BulkUploadAddDraftApprenticeshipsCommand>())).ReturnsAsync(() => DraftApprenticeshipDetails);
            CohortDomainService.Setup(x => x.AddDraftApprenticeship(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<DraftApprenticeshipDetails>(), It.IsAny<UserInfo>(), It.IsAny<CancellationToken>()));

            Handler = new BulkUploadAddDraftApprenticeshipCommandHandler(Mock.Of<ILogger<BulkUploadAddDraftApprenticeshipCommandHandler>>(), ModelMapper.Object, CohortDomainService.Object, ReservationApiClient.Object);
            CancellationToken = new CancellationToken();
        }

        public Task Handle()
        {
            return Handler.Handle(Command, CancellationToken);
        }

        internal void VerifyMapperIsCalled()
        {
            ModelMapper.Verify(x => x.Map<List<DraftApprenticeshipDetails>>(Command), Times.Once);
        }

        internal void VerifyDraftApprenticeshipsAreAdded()
        {
            foreach (var draftApp in DraftApprenticeshipDetails)
            {
                CohortDomainService.Verify(x => x.AddDraftApprenticeship(Command.ProviderId, It.IsAny<long>(), draftApp, Command.UserInfo, It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}