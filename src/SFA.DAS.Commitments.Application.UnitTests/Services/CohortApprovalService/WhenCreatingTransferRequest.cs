using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Services.CohortApprovalService
{
    [TestFixture]
    public class WhenCreatingTransferRequest
    {
        private Application.Services.CohortApprovalService _cohortApprovalService;
        private Mock<ICommitmentRepository> _commitmentRepository;
        private Mock<IApprenticeshipInfoService> _apprenticeshipInfoService;
        private FundingPeriod _fundingPeriod;

        private Commitment _commitment;

        [SetUp]
        public void Arrange()
        {
            _commitmentRepository = new Mock<ICommitmentRepository>();
            _commitmentRepository.Setup(x =>
                x.StartTransferRequestApproval(
                    It.IsAny<long>(),
                    It.IsAny<decimal>(),
                    It.IsAny<int>(),
                    It.IsAny<List<TrainingCourseSummary>>()))
                .ReturnsAsync(1);

            _fundingPeriod = new FundingPeriod {FundingCap = 2000};

            _apprenticeshipInfoService = new Mock<IApprenticeshipInfoService>();
            _apprenticeshipInfoService.Setup(x => x.GetTrainingProgram(It.IsAny<string>()))
                .ReturnsAsync(new Standard
                {
                    FundingPeriods = new List<FundingPeriod>
                    {
                        _fundingPeriod
                    }
                });

            _cohortApprovalService = new Application.Services.CohortApprovalService(
                    Mock.Of<IApprenticeshipRepository>(),
                    Mock.Of<IApprenticeshipOverlapRules>(),
                    Mock.Of<ICurrentDateTime>(),
                    _commitmentRepository.Object,
                    Mock.Of<IApprenticeshipEventsList>(),
                    Mock.Of<IApprenticeshipEventsPublisher>(),
                    Mock.Of<IMediator>(),
                    Mock.Of<ICommitmentsLogger>(),
                    _apprenticeshipInfoService.Object
                );

            _commitment = new Commitment
            {
                TransferSenderId = 1,
                Apprenticeships = new List<Apprenticeship>
                {
                    new Apprenticeship
                    {
                        TrainingCode = "TEST1",
                        StartDate = new DateTime(2018,9,1),
                        Cost = 1000
                    },
                    new Apprenticeship
                    {
                        TrainingCode = "TEST2",
                        StartDate = new DateTime(2018,9,1),
                        Cost = 2000
                    }
                }
            };
        }

        [TestCase(1000, 2000, 5000, 3000, Description = "All costs under cap")]
        [TestCase(1000, 2000, 2000, 3000, Description = "Costs equal to and below cap")]
        [TestCase(1000, 2000, 1000, 2000, Description = "One cost above cap - capping is applied")]
        [TestCase(3000, 5000, 2000, 4000, Description = "All costs above cap - capping is applied")]
        public async Task ThenTheTotalCostShouldBeCalculatedCorrectly(decimal cost1, decimal cost2, int cap, decimal expectedTotal)
        {
            //Arrange
            _commitment = new Commitment
            {
                TransferSenderId = 1,
                Apprenticeships = new List<Apprenticeship>
                {
                    new Apprenticeship
                    {
                        StartDate = new DateTime(2018,9,1),
                        Cost = cost1
                    },
                    new Apprenticeship
                    {
                        StartDate = new DateTime(2018,9,1),
                        Cost = cost2
                    }
                }
            };

            _fundingPeriod.FundingCap = cap;

            //Act
            await _cohortApprovalService.CreateTransferRequest(
                TestHelper.Clone(_commitment), Mock.Of<IMessagePublisher>());

            //test code sums all apprenticeship costs
            _commitmentRepository.Verify(x => x.StartTransferRequestApproval(
                It.IsAny<long>(),
                It.Is<decimal>(cost => cost == expectedTotal),
                It.IsAny<int>(),
                It.IsAny<List<TrainingCourseSummary>>()
                ));
        }
        
        [Test]
        public async Task ThenTheTotalFundingCapShouldBeCalculatedCorrectly()
        {
            //Arrange
            _apprenticeshipInfoService.Setup(x => x.GetTrainingProgram(It.Is<string>(t => t == "TEST1")))
                .ReturnsAsync(new Standard
                {
                    FundingPeriods = new List<FundingPeriod>
                    {
                        new FundingPeriod
                        {
                            FundingCap = 2000
                        }
                    }
                });

            _apprenticeshipInfoService.Setup(x => x.GetTrainingProgram(It.Is<string>(t => t == "TEST2")))
                .ReturnsAsync(new Standard
                {
                    FundingPeriods = new List<FundingPeriod>
                    {
                        new FundingPeriod
                        {
                            EffectiveFrom = new DateTime(2018, 1, 1),
                            EffectiveTo = new DateTime(2018,6,30),
                            FundingCap = 3000
                        },
                        new FundingPeriod
                        {
                            EffectiveFrom = new DateTime(2018, 7, 1),
                            FundingCap = 4000
                        }
                    }
                });

            _commitment = new Commitment
            {
                TransferSenderId = 1,
                Apprenticeships = new List<Apprenticeship>
                {
                    new Apprenticeship
                    {
                        TrainingCode = "TEST1",
                        StartDate = new DateTime(2018,9,1),
                        Cost = 0
                    },
                    new Apprenticeship
                    {
                        TrainingCode = "TEST2",
                        StartDate = new DateTime(2018,1,1),
                        Cost = 0
                    },
                    new Apprenticeship
                    {
                        TrainingCode = "TEST2",
                        StartDate = new DateTime(2018,9,1),
                        Cost = 0
                    }
                }
            };

            //Act
            await _cohortApprovalService.CreateTransferRequest(
                TestHelper.Clone(_commitment), Mock.Of<IMessagePublisher>());

            //Assert
            _commitmentRepository.Verify(x => x.StartTransferRequestApproval(
                It.IsAny<long>(),
                It.IsAny<decimal>(),
                It.Is<int>(cap => cap == 9000),
                It.IsAny<List<TrainingCourseSummary>>()
            ));
        }
    }
}
