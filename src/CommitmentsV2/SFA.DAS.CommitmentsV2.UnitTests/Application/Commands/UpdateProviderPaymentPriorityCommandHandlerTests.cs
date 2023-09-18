using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateProviderPaymentsPriority;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.CommitmentsV2.Messages.Events;
using Newtonsoft.Json;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class UpdateProviderPaymentPriorityCommandHandlerTests
    {
        [TestCaseSource(typeof(UpdateProviderPaymentsPriorityDataCases))]
        public async Task Handle_WhenRequested_ThenItShouldUpdateThePriorityOrder(long accountId, 
            UpdateProviderPaymentsPriorityDataCases.Setup[] setups,
            UpdateProviderPaymentsPriorityDataCases.Input[] inputs,
            UpdateProviderPaymentsPriorityDataCases.ExpectedOutput[] expectedOutputs)
        {
            using var fixture = new UpdateProviderPaymentPriorityCommandHandlerTestsFixture();
            foreach (var setup in setups)
            {
                fixture.SetAccount(setup.AccountId, setup.ProviderId, setup.AccountName, setup.PriorityOrder);
            }

            var updateItems = inputs.Select(input => new UpdateProviderPaymentsPriorityCommand.ProviderPaymentPriorityUpdateItem
            {
                ProviderId = input.ProviderId,
                PriorityOrder = input.PriorityOrder
            }).ToList();

            await fixture.Handle(accountId, updateItems, new UserInfo());
            fixture.VerifyAccount(accountId, expectedOutputs
                .Where(p => p.UpdateType != UpdateProviderPaymentsPriorityDataCases.UpdateType.Removed)
                .Select(expectedOutput => new CustomProviderPaymentPriority
                {
                    EmployerAccountId = expectedOutput.AccountId,
                    ProviderId = expectedOutput.ProviderId,
                    PriorityOrder = expectedOutput.PriorityOrder
                }).ToList());
        }


        [TestCaseSource(typeof(UpdateProviderPaymentsPriorityDataCases))]
        public async Task Handle_WhenRequested_ThenShouldPublishUpdatedStateChangedEvents(long accountId,
            UpdateProviderPaymentsPriorityDataCases.Setup[] setups,
            UpdateProviderPaymentsPriorityDataCases.Input[] inputs,
            UpdateProviderPaymentsPriorityDataCases.ExpectedOutput[] expectedOutputs)
        {
            // Arrange
            using var fixture = new UpdateProviderPaymentPriorityCommandHandlerTestsFixture();
            foreach (var setup in setups)
            {
                fixture.SetAccount(setup.AccountId, setup.ProviderId, setup.AccountName, setup.PriorityOrder);
            }

            var updateItems = inputs.Select(input => new UpdateProviderPaymentsPriorityCommand.ProviderPaymentPriorityUpdateItem
            {
                ProviderId = input.ProviderId,
                PriorityOrder = input.PriorityOrder
            }).ToList();

            // Act
            await fixture.Handle(accountId, updateItems, new UserInfo());

            // Assert
            var changedEntityStateChangedEvents = expectedOutputs
                .Where(p => p.UpdateType == UpdateProviderPaymentsPriorityDataCases.UpdateType.Changed)
                .Select(p => new EntityStateChangedEvent
                {
                    EmployerAccountId = accountId,
                    ProviderId = p.ProviderId,
                    InitialState = JsonConvert.SerializeObject(new CustomProviderPaymentPriority { EmployerAccountId = accountId, ProviderId = p.ProviderId, PriorityOrder = p.OriginalPriorityOrder }),
                    UpdatedState = JsonConvert.SerializeObject(new CustomProviderPaymentPriority { EmployerAccountId = accountId, ProviderId = p.ProviderId, PriorityOrder = p.PriorityOrder }),
                });

            if (changedEntityStateChangedEvents.Any())
            {
                fixture.VerifyUpdatedEntityStateChangedEventPublished(changedEntityStateChangedEvents.ToList());
            }
        }

        [TestCaseSource(typeof(UpdateProviderPaymentsPriorityDataCases))]
        public async Task Handle_WhenRequested_ThenShouldPublishAddedStateChangedEvents(long accountId,
            UpdateProviderPaymentsPriorityDataCases.Setup[] setups,
            UpdateProviderPaymentsPriorityDataCases.Input[] inputs,
            UpdateProviderPaymentsPriorityDataCases.ExpectedOutput[] expectedOutputs)
        {
            // Arrange
            using var fixture = new UpdateProviderPaymentPriorityCommandHandlerTestsFixture();
            foreach (var setup in setups)
            {
                fixture.SetAccount(setup.AccountId, setup.ProviderId, setup.AccountName, setup.PriorityOrder);
            }

            var updateItems = inputs.Select(input => new UpdateProviderPaymentsPriorityCommand.ProviderPaymentPriorityUpdateItem
            {
                ProviderId = input.ProviderId,
                PriorityOrder = input.PriorityOrder
            }).ToList();

            // Act
            await fixture.Handle(accountId, updateItems, new UserInfo());

            // Assert
            var addedEntityStateChangedEvents = expectedOutputs
                .Where(p => p.UpdateType == UpdateProviderPaymentsPriorityDataCases.UpdateType.Added)
                .Select(p => new EntityStateChangedEvent
                {
                    EmployerAccountId = accountId,
                    ProviderId = p.ProviderId,
                    InitialState = null,
                    UpdatedState = JsonConvert.SerializeObject(new CustomProviderPaymentPriority { EmployerAccountId = accountId, ProviderId = p.ProviderId, PriorityOrder = p.PriorityOrder }),
                });

            if (addedEntityStateChangedEvents.Any())
            {
                fixture.VerifyUpdatedEntityStateChangedEventPublished(addedEntityStateChangedEvents.ToList());
            }
        }

        [TestCaseSource(typeof(UpdateProviderPaymentsPriorityDataCases))]
        public async Task Handle_WhenRequested_ThenShouldPublishRemovedStateChangedEvents(long accountId,
            UpdateProviderPaymentsPriorityDataCases.Setup[] setups,
            UpdateProviderPaymentsPriorityDataCases.Input[] inputs,
            UpdateProviderPaymentsPriorityDataCases.ExpectedOutput[] expectedOutputs)
        {
            // Arrange
            using var fixture = new UpdateProviderPaymentPriorityCommandHandlerTestsFixture();
            foreach (var setup in setups)
            {
                fixture.SetAccount(setup.AccountId, setup.ProviderId, setup.AccountName, setup.PriorityOrder);
            }

            var updateItems = inputs.Select(input => new UpdateProviderPaymentsPriorityCommand.ProviderPaymentPriorityUpdateItem
            {
                ProviderId = input.ProviderId,
                PriorityOrder = input.PriorityOrder
            }).ToList();

            // Act
            await fixture.Handle(accountId, updateItems, new UserInfo());

            // Assert
            var removedEntityStateChangedEvents = expectedOutputs
                .Where(p => p.UpdateType == UpdateProviderPaymentsPriorityDataCases.UpdateType.Removed)
                .Select(p => new EntityStateChangedEvent
                {
                    EmployerAccountId = accountId,
                    ProviderId = p.ProviderId,
                    InitialState = JsonConvert.SerializeObject(new CustomProviderPaymentPriority { EmployerAccountId = accountId, ProviderId = p.ProviderId, PriorityOrder = p.OriginalPriorityOrder }),
                    UpdatedState = null,
                });

            if (removedEntityStateChangedEvents.Any())
            {
                fixture.VerifyUpdatedEntityStateChangedEventPublished(removedEntityStateChangedEvents.ToList());
            }
        }

        [TestCaseSource(typeof(UpdateProviderPaymentsPriorityDataCases))]
        public async Task Handle_WhenRequested_ThenShouldPublishPaymentOrderChangedEvent(long accountId,
            UpdateProviderPaymentsPriorityDataCases.Setup[] setups,
            UpdateProviderPaymentsPriorityDataCases.Input[] inputs,
            UpdateProviderPaymentsPriorityDataCases.ExpectedOutput[] expectedOutputs)
        {
            // Arrange
            using var fixture = new UpdateProviderPaymentPriorityCommandHandlerTestsFixture();
            foreach (var setup in setups)
            {
                fixture.SetAccount(setup.AccountId, setup.ProviderId, setup.AccountName, setup.PriorityOrder);
            }

            var updateItems = inputs.Select(input => new UpdateProviderPaymentsPriorityCommand.ProviderPaymentPriorityUpdateItem
            {
                ProviderId = input.ProviderId,
                PriorityOrder = input.PriorityOrder
            }).ToList();

            // Act
            await fixture.Handle(accountId, updateItems, new UserInfo());

            // Assert
            var entityStateChanged = expectedOutputs
                .Any(p => p.UpdateType != UpdateProviderPaymentsPriorityDataCases.UpdateType.None);

            if (entityStateChanged)
            {
                var paymentOrder = expectedOutputs
                    .Where(p => p.UpdateType != UpdateProviderPaymentsPriorityDataCases.UpdateType.Removed)
                    .OrderBy(x => x.PriorityOrder)
                    .Select(x => (int)x.ProviderId);

                fixture.VerifyPaymentOrderChangedEventPublished(accountId, paymentOrder.ToArray());
            }
        }
    }

    public class UpdateProviderPaymentsPriorityDataCases : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            var provider101 = new { ProviderId = 101 };
            var provider102 = new { ProviderId = 102 };
            var provider103 = new { ProviderId = 103 };

            // current custom provider payment priority ordered by P101, P102, P103
            yield return new object[] {
                111111,
                new Setup[]
                {
                    new Setup { AccountId = 111111, PriorityOrder = 1, ProviderId = provider101.ProviderId},
                    new Setup { AccountId = 111111, PriorityOrder = 2, ProviderId = provider102.ProviderId},
                    new Setup { AccountId = 111111, PriorityOrder = 3, ProviderId = provider103.ProviderId}
                },
                new Input[]
                {
                    new Input { ProviderId = provider101.ProviderId, PriorityOrder = 1},
                    new Input { ProviderId = provider102.ProviderId, PriorityOrder = 2},
                    new Input { ProviderId = provider103.ProviderId, PriorityOrder = 3}
                },
                new ExpectedOutput[]
                {
                    new ExpectedOutput { AccountId = 111111, ProviderId = provider101.ProviderId, PriorityOrder = 1, OriginalPriorityOrder = 1, UpdateType = UpdateType.None},
                    new ExpectedOutput { AccountId = 111111, ProviderId = provider102.ProviderId, PriorityOrder = 2, OriginalPriorityOrder = 2, UpdateType = UpdateType.None},
                    new ExpectedOutput { AccountId = 111111, ProviderId = provider103.ProviderId, PriorityOrder = 3, OriginalPriorityOrder = 3, UpdateType = UpdateType.None}
                }
            };

            // current custom provider payment priority ordered by P103, P102, P101
            yield return new object[] {
                222222,
                new Setup[]
                {
                    new Setup { AccountId = 222222, PriorityOrder = 3, ProviderId = provider101.ProviderId},
                    new Setup { AccountId = 222222, PriorityOrder = 2, ProviderId = provider102.ProviderId},
                    new Setup { AccountId = 222222, PriorityOrder = 1, ProviderId = provider103.ProviderId}
                },
                new Input[]
                {
                    new Input { ProviderId = provider101.ProviderId, PriorityOrder = 1},
                    new Input { ProviderId = provider102.ProviderId, PriorityOrder = 2},
                    new Input { ProviderId = provider103.ProviderId, PriorityOrder = 3}
                },
                new ExpectedOutput[]
                {
                    new ExpectedOutput { AccountId = 222222, ProviderId = provider101.ProviderId, PriorityOrder = 1, OriginalPriorityOrder = 3, UpdateType = UpdateType.Changed},
                    new ExpectedOutput { AccountId = 222222, ProviderId = provider102.ProviderId, PriorityOrder = 2, OriginalPriorityOrder = 2, UpdateType = UpdateType.None},
                    new ExpectedOutput { AccountId = 222222, ProviderId = provider103.ProviderId, PriorityOrder = 3, OriginalPriorityOrder = 1, UpdateType = UpdateType.Changed}
                }
            };

            // partial custom provider payment priority ordered by P103, P101
            yield return new object[] {
                333333,
                new Setup[]
                {
                    new Setup { AccountId = 333333, PriorityOrder = 3, ProviderId = provider101.ProviderId},
                    new Setup { AccountId = 333333, PriorityOrder = 1, ProviderId = provider103.ProviderId}
                },
                new Input[]
                {
                    new Input { ProviderId = provider101.ProviderId, PriorityOrder = 1},
                    new Input { ProviderId = provider102.ProviderId, PriorityOrder = 2},
                    new Input { ProviderId = provider103.ProviderId, PriorityOrder = 3}
                },
                new ExpectedOutput[]
                {
                    new ExpectedOutput { AccountId = 333333, ProviderId = provider101.ProviderId, PriorityOrder = 1, OriginalPriorityOrder = 3, UpdateType = UpdateType.Changed},
                    new ExpectedOutput { AccountId = 333333, ProviderId = provider102.ProviderId, PriorityOrder = 2, OriginalPriorityOrder = 2, UpdateType = UpdateType.Added},
                    new ExpectedOutput { AccountId = 333333, ProviderId = provider103.ProviderId, PriorityOrder = 3, OriginalPriorityOrder = 1, UpdateType = UpdateType.Changed}
                }
            };

            // partial custom provider payment priority ordered by P103, P101
            yield return new object[] {
                444444,
                new Setup[]
                {
                    new Setup { AccountId = 444444, PriorityOrder = 3, ProviderId = provider101.ProviderId},
                    new Setup { AccountId = 444444, PriorityOrder = 2, ProviderId = provider102.ProviderId},
                    new Setup { AccountId = 444444, PriorityOrder = 1, ProviderId = provider103.ProviderId}
                },
                new Input[]
                {
                    new Input { ProviderId = provider101.ProviderId, PriorityOrder = 1},
                    new Input { ProviderId = provider102.ProviderId, PriorityOrder = 2},
                },
                new ExpectedOutput[]
                {
                    new ExpectedOutput { AccountId = 444444, ProviderId = provider101.ProviderId, PriorityOrder = 1, OriginalPriorityOrder = 3, UpdateType = UpdateType.Changed},
                    new ExpectedOutput { AccountId = 444444, ProviderId = provider102.ProviderId, PriorityOrder = 2, OriginalPriorityOrder = 2, UpdateType = UpdateType.None},
                    new ExpectedOutput { AccountId = 444444, ProviderId = provider103.ProviderId, PriorityOrder = 3, OriginalPriorityOrder = 1, UpdateType = UpdateType.Removed}
                }
            };
        }

        public class Setup
        {
            public long AccountId { get; set; }
            public string AccountName { get; set; }
            public long ProviderId { get; set; }
            public int? PriorityOrder { get; set; }
        }

        public class Input
        {
            public long ProviderId { get; set; }
            public int PriorityOrder { get; set; }
        }

        public class ExpectedOutput
        {
            public UpdateType UpdateType { get; set; }
            public long AccountId { get; set; }
            public long ProviderId { get; set; }
            public int PriorityOrder { get; set; }
            public int OriginalPriorityOrder { get; set; }
        }

        public enum UpdateType
        {
            None,
            Changed,
            Added,
            Removed
        }
    }

    public class UpdateProviderPaymentPriorityCommandHandlerTestsFixture : IDisposable
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        
        public Mock<ILogger<UpdateProviderPaymentsPriorityCommandHandler>> Logger;
        public IRequestHandler<UpdateProviderPaymentsPriorityCommand> Handler { get; set; }

        public UnitOfWorkContext UnitOfWorkContext { get; set; }

        public UpdateProviderPaymentPriorityCommandHandlerTestsFixture()
        {
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            
            Logger = new Mock<ILogger<UpdateProviderPaymentsPriorityCommandHandler>>();

            UnitOfWorkContext = new UnitOfWorkContext();

            Handler = new UpdateProviderPaymentsPriorityCommandHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db), Logger.Object);
        }

        public async Task Handle(long accountId, List<UpdateProviderPaymentsPriorityCommand.ProviderPaymentPriorityUpdateItem> updateItems, UserInfo userInfo)
        {
            var request = new UpdateProviderPaymentsPriorityCommand(accountId, updateItems, userInfo);
            await Handler.Handle(request, CancellationToken.None);

            // this call is part of the DAS.SFA.UnitOfWork.Context.UnitOfWorkContext middleware in the API
            await Db.SaveChangesAsync();
        }

        public void SetAccount(long accountId, long providerId, string accountName, int? priorityOrder)
        {
            var autoFixture = new Fixture();

            if (!Db.Accounts.Any(p => p.Id == accountId))
            {
                var account = new Account(
                    accountId,
                    autoFixture.Create<string>(),
                    autoFixture.Create<string>(),
                    accountName,
                    DateTime.UtcNow);

                Db.Accounts.Add(account);
                Db.SaveChanges();
            }

            if (priorityOrder.HasValue)
            {
                if (!Db.CustomProviderPaymentPriorities.Any(p => p.EmployerAccountId == accountId && p.ProviderId == providerId))
                {
                    var customProviderPaymentPriority = new CustomProviderPaymentPriority
                    {
                        ProviderId = providerId,
                        EmployerAccountId = accountId,
                        PriorityOrder = priorityOrder.Value
                    };

                    Db.CustomProviderPaymentPriorities.Add(customProviderPaymentPriority);
                    Db.SaveChanges();
                }
            }
        }

        public void VerifyAccount(long accountId, List<CustomProviderPaymentPriority> customProviderPaymentPriorities)
        {
            TestHelpers.CompareHelper.AreEqualIgnoringTypes(Db.Accounts.FirstOrDefault(p => p.Id == accountId)?.CustomProviderPaymentPriorities,
                customProviderPaymentPriorities).Should().BeTrue();
        }

        public void VerifyUpdatedEntityStateChangedEventPublished(List<EntityStateChangedEvent> updatedEntityStateChangedEvents)
        {
            foreach(var updatedItem in updatedEntityStateChangedEvents)
            {
                var entityStateChangedEvents = UnitOfWorkContext
                    .GetEvents()
                    .OfType<EntityStateChangedEvent>()
                    .Where(p =>
                        p.EmployerAccountId == updatedItem.EmployerAccountId &&
                        p.ProviderId == updatedItem.ProviderId &&
                        p.InitialState == updatedItem.InitialState &&
                        p.UpdatedState == updatedItem.UpdatedState)
                    .Count()
                    .Should()
                    .Be(1);
            }
        }

        public void VerifyPaymentOrderChangedEventPublished(long accountId, int[] paymentOrder)
        {
            var paymentOrderChangedEvent = UnitOfWorkContext.GetEvents().OfType<PaymentOrderChangedEvent>().First();

            paymentOrderChangedEvent.Should().BeEquivalentTo(new PaymentOrderChangedEvent
            {
                AccountId = accountId,
                PaymentOrder = paymentOrder
            });
        }

        public void Dispose()
        {
            Db?.Dispose();
        }
    }
}
