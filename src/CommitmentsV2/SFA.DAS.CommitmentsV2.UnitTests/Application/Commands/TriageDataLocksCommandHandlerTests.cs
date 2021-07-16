using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Application.Commands.TriageDataLocks;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class TriageDataLocksCommandHandlerTests
    {
        ProviderCommitmentsDbContext Db;
        ProviderCommitmentsDbContext DbConfirm;
        ProviderCommitmentsDbContext _dbContext;
        ProviderCommitmentsDbContext _confirmationDbContext;
        private Mock<IAuthenticationService> _authenticationService;
        private Mock<ILogger<TriageDataLocksCommandHandler>> _logger;
        private IRequestHandler<TriageDataLocksCommand> _handler;
        private TriageDataLocksCommand _validCommand;
        public UserInfo UserInfo;
        public Fixture AutoFixture { get; set; }
        private UnitOfWorkContext _unitOfWorkContext { get; set; }

        [SetUp]
        public void Init()
        {
            var databaseGuid = Guid.NewGuid().ToString();            
            
            _dbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                        .UseInMemoryDatabase(databaseGuid)
                                        .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                                        .Options);

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()               
               .UseInMemoryDatabase(databaseGuid)
               .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
               .Options);

            DbConfirm = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()               
               .UseInMemoryDatabase(databaseGuid)
               .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
               .Options);

            _confirmationDbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                            .UseInMemoryDatabase(databaseGuid)
                            .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                            .Options);

            _unitOfWorkContext = new UnitOfWorkContext();
            _authenticationService = new Mock<IAuthenticationService>();
            _logger = new Mock<ILogger<TriageDataLocksCommandHandler>>();

            _handler = new TriageDataLocksCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db),
                _logger.Object,
                _authenticationService.Object);

        }

        [Test]
        public async Task Should_Triage_DataLocks()
        {
            //Arrange
            var apprenticeship = SeedData(false);
            var fixture = new Fixture();
            UserInfo = fixture.Create<UserInfo>();

            _validCommand = new TriageDataLocksCommand(10082, TriageStatus.Restart, UserInfo);            

            //Act
            await _handler.Handle(_validCommand, default);
            await Db.SaveChangesAsync();

            //Assert
            var apprenticeshipAssertion = await Db.Apprenticeships.FirstAsync(a => a.Id == apprenticeship.Id);
            Assert.AreEqual(TriageStatus.Restart , apprenticeshipAssertion.DataLockStatus.FirstOrDefault().TriageStatus);
        }

        [Test]
        public void Should_Not_Update_If_Request_Has_Same_TriageStatus()
        {
            //Arrange

            var fixture = new Fixture();
            var apprenticeshipId = 10082;
            var apprenticeship = new Apprenticeship
            {
                Id = apprenticeshipId,                
                Cohort = new Cohort
                {
                    EmployerAccountId = fixture.Create<long>(),
                    AccountLegalEntity = new AccountLegalEntity()
                },
                DataLockStatus = new List<DataLockStatus>()
                {
                    new DataLockStatus
                    {
                        ApprenticeshipId = apprenticeshipId,
                        DataLockEventId = 1,
                        EventStatus = EventStatus.New,
                        IsExpired = false,
                        TriageStatus = TriageStatus.Change,
                        ErrorCode = DataLockErrorCode.Dlock04
                    }
                },
                StartDate = DateTime.UtcNow.AddMonths(-2)
            };
            Db.Apprenticeships.Add(apprenticeship);
            Db.SaveChanges();
            UserInfo = fixture.Create<UserInfo>();
            _validCommand = new TriageDataLocksCommand(10082, TriageStatus.Change, UserInfo);
            var expectedMessage = $"Trying to update data lock for apprenticeship: {_validCommand.ApprenticeshipId} with the same TriageStatus ({_validCommand.TriageStatus}) ";          

            //Act
            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _handler.Handle(_validCommand, new CancellationToken()));

            //Assert
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        public async Task Should_Ignore_Passed_Datalocks()
        {
            //Arrange
            var fixture = new Fixture();
            var apprenticeshipId = 10082;
            var apprenticeship = new Apprenticeship
            {
                Id = apprenticeshipId,
                HasHadDataLockSuccess = true,
                Cohort = new Cohort
                {
                    EmployerAccountId = fixture.Create<long>(),
                    AccountLegalEntity = new AccountLegalEntity()
                },
                DataLockStatus = new List<DataLockStatus>()
                {
                    new DataLockStatus
                    {
                        ApprenticeshipId = apprenticeshipId,
                        DataLockEventId = 1,
                        TriageStatus = TriageStatus.Change,
                        ErrorCode = DataLockErrorCode.Dlock04,
                        Status = Status.Pass
                    }
                },
                StartDate = DateTime.UtcNow.AddMonths(-2)
            };
            Db.Apprenticeships.Add(apprenticeship);
            Db.SaveChanges();
            UserInfo = fixture.Create<UserInfo>();
            _validCommand = new TriageDataLocksCommand(10082, TriageStatus.Restart, UserInfo);


            //Act
            await _handler.Handle(_validCommand, default);

            //Assert
            _unitOfWorkContext.GetEvents().OfType<EntityStateChangedEvent>().Should().BeEmpty();
        }


        [Test]
        public void Should_Not_Update_CourseDataLock_If_Apprenticeship_HasHadSuccessful_DataLock()
        {

            //Arrange
            var fixture = new Fixture();
            var apprenticeshipId = 10082;
            var apprenticeship = new Apprenticeship
            {
                Id = apprenticeshipId,
                HasHadDataLockSuccess = true,
                Cohort = new Cohort
                {
                    EmployerAccountId = fixture.Create<long>(),
                    AccountLegalEntity = new AccountLegalEntity()
                },
                DataLockStatus = new List<DataLockStatus>()
                {
                    new DataLockStatus {   ErrorCode = (DataLockErrorCode)68}
                },                
                StartDate = DateTime.UtcNow.AddMonths(-2)
            };
            Db.Apprenticeships.Add(apprenticeship);
            Db.SaveChanges();
            UserInfo = fixture.Create<UserInfo>();            
            _validCommand = new TriageDataLocksCommand(10082, TriageStatus.Change, UserInfo);
            var expectedMessage = $"Trying to update data lock for apprenticeship: {_validCommand.ApprenticeshipId} with triage status ({_validCommand.TriageStatus}) and datalock with course and price when Successful DataLock already received";

            //Act
            var exception =  Assert.ThrowsAsync<InvalidOperationException>(async () => await _handler.Handle(_validCommand, new CancellationToken()));
            
            //Assert         
            Assert.AreEqual(expectedMessage, exception.Message);
        }


        public Apprenticeship SeedData(bool withPriceHistory = true)
        {
            var accountLegalEntityDetails = new AccountLegalEntity()
                .Set(c => c.Id, 444);

            Db.AccountLegalEntities.Add(accountLegalEntityDetails);

            var cohortDetails = new Cohort()
                .Set(c => c.Id, 111)
                .Set(c => c.EmployerAccountId, 222)
                .Set(c => c.ProviderId, 333)
                .Set(c => c.AccountLegalEntityId, accountLegalEntityDetails.Id);

            Db.Cohorts.Add(cohortDetails);

            if (withPriceHistory)
            {
                var priceHistoryDetails = new List<PriceHistory>()
                {
                    new PriceHistory
                    {
                        FromDate = DateTime.Now,
                        ToDate = null,
                        Cost = 10000,
                    }
                };

                Db.PriceHistory.AddRange(priceHistoryDetails);
            }

            var fixture = new Fixture();
            var apprenticeshipId = 10082;
            var apprenticeship = new Apprenticeship
            {
                Id = apprenticeshipId,
                Cohort = new Cohort
                {
                    EmployerAccountId = fixture.Create<long>(),
                    AccountLegalEntity = new AccountLegalEntity()
                },
                DataLockStatus = SetupDataLocks(apprenticeshipId),                
                StartDate = DateTime.UtcNow.AddMonths(-2)
            };            
            apprenticeship.CommitmentId = cohortDetails.Id;

            Db.Apprenticeships.Add(apprenticeship);
            Db.SaveChanges();

            return apprenticeship;           
        }

        private ICollection<DataLockStatus> SetupDataLocks(long apprenticeshipId)
        {
            var activeDataLock4 = new DataLockStatus
            {
                ApprenticeshipId = apprenticeshipId,
                DataLockEventId = 1,
                EventStatus = EventStatus.New,
                IsExpired = false,
                TriageStatus = TriageStatus.Unknown,
                ErrorCode = DataLockErrorCode.Dlock04
            };
            
            return new List<DataLockStatus> { activeDataLock4};
        }
       
    }
}
