using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Data
{
    [TestFixture]
    [Parallelizable]
    public class HistoryUnitOfWorkTests
    {
        private HistoryUnitOfWorkTestsFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new HistoryUnitOfWorkTestsFixture();
        }

        [Test]
        public void CommitAsync_WhenEntityStateChangesExist_ThenShouldAddHistoryItems()
        {
            _fixture.SetEntityStateChanges()
                .CommitAsync();
            
            _fixture.Db.Object.HistoryItemsV2.Should().HaveSameCount(_fixture.HistoryItems)
                .And.BeEquivalentTo(_fixture.HistoryItems);
        }
    }

    public class HistoryUnitOfWorkTestsFixture
    {
        public IFixture AutoFixture { get; set; }
        public DateTime Now { get; set; }
        public Mock<FoobarCommitmentsDbContext> Db { get; set; }
        public Mock<DatabaseFacade> Database { get; set; }
        public Mock<ICurrentDateTime> CurrentDateTime { get; set; }
        public HistoryUnitOfWork UnitOfWork { get; set; }
        public List<HistoryItemV2> HistoryItems { get; set; }

        public HistoryUnitOfWorkTestsFixture()
        {
            AutoFixture = new Fixture();
            Now = DateTime.UtcNow;
            Db = new Mock<FoobarCommitmentsDbContext>(new DbContextOptionsBuilder<CommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options) { CallBase = true };
            Database = new Mock<DatabaseFacade>(Db.Object);
            CurrentDateTime = new Mock<ICurrentDateTime>();
            UnitOfWork = new HistoryUnitOfWork(new Lazy<CommitmentsDbContext>(() => Db.Object), CurrentDateTime.Object);
            HistoryItems = new List<HistoryItemV2>();

            Database.Setup(d => d.CurrentTransaction.TransactionId).Returns(Guid.NewGuid());
            Db.Setup(d => d.Database).Returns(Database.Object);
            CurrentDateTime.Setup(c => c.UtcNow).Returns(Now);
        }

        public Task CommitAsync()
        {
            return UnitOfWork.CommitAsync(() => Db.Object.SaveChangesAsync());
        }

        public HistoryUnitOfWorkTestsFixture SetEntityStateChanges()
        {
            var delete = AutoFixture.Create<Foobar>();
            var modify = AutoFixture.Create<Foobar>();
            var add = AutoFixture.Create<Foobar>();

            Db.Object.Foobars.AddRange(delete, modify);
            Db.Object.SaveChanges();
            Db.Object.Foobars.Add(add);
            Db.Object.Foobars.Remove(delete);
            
            HistoryItems.AddRange(new []
            {
                new HistoryItemV2
                {
                    Id = 1,
                    TransactionId = Db.Object.Database.CurrentTransaction.TransactionId,
                    EntityType = delete.GetType().FullName,
                    EntityState = EntityState.Deleted.ToString(),
                    Original = delete.ToJson(),
                    Modified = null,
                    CreatedOn = Now
                },
                new HistoryItemV2
                {
                    Id = 2,
                    TransactionId = Db.Object.Database.CurrentTransaction.TransactionId,
                    EntityType = modify.GetType().FullName,
                    EntityState = EntityState.Modified.ToString(),
                    Original = modify.ToJson(),
                    Modified = modify.Modify(AutoFixture.Create<string>(), AutoFixture.Create<string>()).ToJson(),
                    CreatedOn = Now
                },
                new HistoryItemV2
                {
                    Id = 3,
                    TransactionId = Db.Object.Database.CurrentTransaction.TransactionId,
                    EntityType = add.GetType().FullName,
                    EntityState = EntityState.Added.ToString(),
                    Original = null,
                    Modified = add.ToJson(),
                    CreatedOn = Now
                }
            });
            
            return this;
        }

        public class FoobarCommitmentsDbContext : CommitmentsDbContext
        {
            public DbSet<Foobar> Foobars { get; set; }

            public FoobarCommitmentsDbContext(DbContextOptions<CommitmentsDbContext> options)
                : base(options)
            {
            }
        }

        public class Foobar
        {
            public int Id { get; protected set; }
            public string Bar { get; protected set; }
            public string Foo { get; protected set; }

            public Foobar(string foo, string bar)
            {
                Foo = foo;
                Bar = bar;
            }

            protected Foobar()
            {
            }

            public object Modify(string foo, string bar)
            {
                Foo = foo;
                Bar = bar;

                return new { Bar, Foo };
            }
        }
    }
}