using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands.UpdateCustomProviderPaymentPriority;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.UpdateProviderPaymentsPriority
{
    [TestFixture]
    public sealed class WhenSavingNewPriorityList
    {
        private UpdateProviderPaymentsPriorityCommand _validCommand;
        private UpdateProviderPaymentsPriorityCommandHandler _handler;
        private Mock<IProviderPaymentRepository> _mockProviderPaymentRepository;

        private Mock<ICommitmentRepository> _mockCommitmentRepository;
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRepository;
        private Mock<IApprenticeshipEventsPublisher> _mockApprenticeshipEventsPublisher;
        private Mock<ICommitmentsLogger> _mockLogger;
        private Mock<IV2EventsPublisher> _mockV2EventsPublisher;

        [SetUp]
        public void Setup()
        {
            _validCommand = CreateValidCommand();

            CreateMockObjects();
            SetUpMocksForApprenticeshipsretreivalBeforeAndAfterReOrder();
            Container container = InitialiseDependencies();
            var validator = new UpdateProviderPaymentsPriorityCommandValidator();

            _handler = new UpdateProviderPaymentsPriorityCommandHandler(validator, _mockProviderPaymentRepository.Object, container.GetInstance<IMediator>(), _mockV2EventsPublisher.Object);
        }

        [Test]
        public async Task ShouldCallTheProviderPaymentRepository()
        {
            await _handler.Handle(_validCommand);

            _mockProviderPaymentRepository.Verify(x => x.UpdateProviderPaymentPriority(It.IsAny<long>(), It.IsAny<IList<ProviderPaymentPriorityUpdateItem>>()));
        }

        [Test]
        public async Task ShouldPublishPaymentOrderChangedEvent()
        {
            await _handler.Handle(_validCommand);

            _mockV2EventsPublisher.Verify(x => x.PublishPaymentOrderChanged(_validCommand.EmployerAccountId, It.Is<IEnumerable<int>>(p=>p.Count() == 3)));
        }

        private void SetUpMocksForApprenticeshipsretreivalBeforeAndAfterReOrder()
        {
            var intialApprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship { Id = 1, PaymentOrder = 1 },
                new Apprenticeship { Id = 2, PaymentOrder = 2 },
                new Apprenticeship { Id = 3, PaymentOrder = 3 },
            };

            var afterUpdateApprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship { Id = 1, CommitmentId = 1, PaymentOrder = 3 }, // Two apprenticeships have changed order
                new Apprenticeship { Id = 2, CommitmentId = 1, PaymentOrder = 2 },
                new Apprenticeship { Id = 3, CommitmentId = 1, PaymentOrder = 1 },
            };

            _mockApprenticeshipRepository.SetupSequence(x => x.GetApprenticeshipsByEmployer(It.IsAny<long>(), It.IsAny<string>()))
                .ReturnsAsync(new ApprenticeshipsResult { Apprenticeships = intialApprenticeships })
                .ReturnsAsync(new ApprenticeshipsResult { Apprenticeships = afterUpdateApprenticeships });

            _mockCommitmentRepository.Setup(x => x.GetCommitmentById(It.IsAny<long>()))
                .ReturnsAsync(new Commitment { Id = 1 });
        }

        private static UpdateProviderPaymentsPriorityCommand CreateValidCommand()
        {
            return new UpdateProviderPaymentsPriorityCommand
            {
                EmployerAccountId = 123L,
                ProviderPriorities = new List<ProviderPaymentPriorityUpdateItem>
                {
                    new ProviderPaymentPriorityUpdateItem { PriorityOrder = 1, ProviderId = 99 },
                    new ProviderPaymentPriorityUpdateItem { PriorityOrder = 2, ProviderId = 22 },
                    new ProviderPaymentPriorityUpdateItem { PriorityOrder = 3, ProviderId = 66 },
                }
            };
        }

        private Container InitialiseDependencies()
        {
            return new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(IAsyncRequestHandler<,>));
                });
                cfg.For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
                cfg.For<IMediator>().Use<Mediator>();

                cfg.For<ICommitmentRepository>().Use(_mockCommitmentRepository.Object);
                cfg.For<IApprenticeshipRepository>().Use(_mockApprenticeshipRepository.Object);
                cfg.For<IApprenticeshipEventsList>().Use(new TestApprenticeshipEventsList());
                cfg.For<IApprenticeshipEventsPublisher>().Use(_mockApprenticeshipEventsPublisher.Object);
                cfg.For<ICommitmentsLogger>().Use(_mockLogger.Object);
                cfg.For<ICurrentDateTime>().Use(Mock.Of<ICurrentDateTime>());
            });
        }

        private void CreateMockObjects()
        {
            _mockProviderPaymentRepository = new Mock<IProviderPaymentRepository>();
            _mockCommitmentRepository = new Mock<ICommitmentRepository>();
            _mockApprenticeshipRepository = new Mock<IApprenticeshipRepository>();
            _mockApprenticeshipEventsPublisher = new Mock<IApprenticeshipEventsPublisher>();
            _mockV2EventsPublisher = new Mock<IV2EventsPublisher>();
            _mockLogger = new Mock<ICommitmentsLogger>();
        }

        private class TestApprenticeshipEventsList : IApprenticeshipEventsList
        {
            private IList<IApprenticeshipEvent> _events = new List<IApprenticeshipEvent>();

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public IReadOnlyList<IApprenticeshipEvent> Events => _events.ToArray();

            public void Add(Commitment commitment, Apprenticeship apprenticeship, string @event, DateTime? effectiveFrom = default(DateTime?), DateTime? effectiveTo = default(DateTime?))
            {
                _events.Add(Mock.Of<IApprenticeshipEvent>());
            }
        }
    }
}
