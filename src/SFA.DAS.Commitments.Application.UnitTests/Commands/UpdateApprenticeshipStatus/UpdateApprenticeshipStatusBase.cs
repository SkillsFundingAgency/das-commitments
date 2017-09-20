﻿using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using System.Collections.Generic;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Infrastructure.Services;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateApprenticeshipStatus
{
    public abstract class UpdateApprenticeshipStatusBase
    {
        protected Mock<ICommitmentRepository> MockCommitmentRespository;
        protected Mock<IApprenticeshipRepository> MockApprenticeshipRespository;
        protected Mock<ICurrentDateTime> MockCurrentDateTime;
        protected Mock<IApprenticeshipEvents> MockEventsApi;
        protected Mock<IHistoryRepository> MockHistoryRepository;
        protected Mock<IDataLockRepository> MockDataLockRepository;
        protected UpdateApprenticeshipStatusCommandHandler Handler;
        protected UpdateApprenticeshipStatusCommand ExampleValidRequest;
        protected Apprenticeship TestApprenticeship;
        protected Mock<IAcademicYearValidator> MockAcademicYearValidator;

        protected abstract PaymentStatus RequestPaymentStatus { get; }
        protected abstract PaymentStatus ApprenticeshipPaymentStatus { get; }

        [SetUp]
        public void SetUp()
        {
            ExampleValidRequest = new UpdateApprenticeshipStatusCommand
            {
                AccountId = 111L,
                ApprenticeshipId = 444L,
                PaymentStatus = RequestPaymentStatus,
                DateOfChange = DateTime.Now.Date,
                UserName = "Bob"
            };

            TestApprenticeship = new Apprenticeship
            {
                CommitmentId = 123L,
                PaymentStatus = ApprenticeshipPaymentStatus,
                StartDate = DateTime.UtcNow.Date.AddMonths(-1)
            };

            MockCommitmentRespository = new Mock<ICommitmentRepository>();
            MockApprenticeshipRespository = new Mock<IApprenticeshipRepository>();

            MockEventsApi = new Mock<IApprenticeshipEvents>();
            MockHistoryRepository = new Mock<IHistoryRepository>();
            MockDataLockRepository = new Mock<IDataLockRepository>();

            MockApprenticeshipRespository.Setup(x => x.GetApprenticeship(It.Is<long>(y => y == ExampleValidRequest.ApprenticeshipId))).ReturnsAsync(TestApprenticeship);
            MockApprenticeshipRespository.Setup(x => x.UpdateApprenticeshipStatus(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<PaymentStatus>())).Returns(Task.FromResult(new object()));
            MockDataLockRepository.Setup(x => x.GetDataLocks(ExampleValidRequest.ApprenticeshipId)).ReturnsAsync(new List<DataLockStatus>());

            MockCurrentDateTime = new Mock<ICurrentDateTime>();
            MockCurrentDateTime.SetupGet(x => x.Now).Returns(DateTime.UtcNow);

            MockAcademicYearValidator = new Mock<IAcademicYearValidator>();

            Handler = new UpdateApprenticeshipStatusCommandHandler(
                MockCommitmentRespository.Object,
                MockApprenticeshipRespository.Object,
                new UpdateApprenticeshipStatusValidator(),
                MockCurrentDateTime.Object,
                MockEventsApi.Object,
                Mock.Of<ICommitmentsLogger>(),
                MockHistoryRepository.Object,
                MockDataLockRepository.Object,
                MockAcademicYearValidator.Object);
        }
    }
}
