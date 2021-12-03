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

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class TriageDataLocksCommandHandlerTests
    {        
        ProviderCommitmentsDbContext _dbContext;
        ProviderCommitmentsDbContext _confirmationDbContext;
        private Mock<IAuthenticationService> _authenticationService;
        private Mock<ILogger<TriageDataLocksCommandHandler>> _logger;
        private IRequestHandler<TriageDataLocksCommand> _handler;
        private TriageDataLocksCommand _validCommand;
        public UserInfo UserInfo;
        public Fixture _fixture;
        private UnitOfWorkContext _unitOfWorkContext { get; set; }
        private long _apprenticeshipId; 

        [SetUp]
        public void Init()
        {
            var databaseGuid = Guid.NewGuid().ToString();
            _fixture = new Fixture();
            _apprenticeshipId = 10082;
            _dbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                        .UseInMemoryDatabase(databaseGuid)
                                        .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                                        .Options);           

            _confirmationDbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                            .UseInMemoryDatabase(databaseGuid)
                            .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                            .Options);

            _unitOfWorkContext = new UnitOfWorkContext();
            _authenticationService = new Mock<IAuthenticationService>();
            _logger = new Mock<ILogger<TriageDataLocksCommandHandler>>();

            _handler = new TriageDataLocksCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => _dbContext),
                _logger.Object,
                _authenticationService.Object);

        }

        [Test]
        public async Task Should_Update_DataLock()
        {
            //Arrange
            var apprenticeship = SetupApprenticeship(TriageStatus.Unknown , false, false);
            _validCommand = new TriageDataLocksCommand(_apprenticeshipId, TriageStatus.Restart, _fixture.Create<UserInfo>());            

            //Act
            await _handler.Handle(_validCommand, default);
            await _dbContext.SaveChangesAsync();

            //Assert          
            var apprenticeshipDataLock = _confirmationDbContext.DataLocks.Where(s => s.ApprenticeshipId == apprenticeship.Id);
            Assert.AreEqual(TriageStatus.Restart, apprenticeshipDataLock.FirstOrDefault().TriageStatus);
        }

        [Test]
        public async Task Should_Update_All_DataLocks()
        {
            //Arrange
            var apprenticeship = SetupApprenticeshipWithDatalocks(TriageStatus.Unknown, false);
            _validCommand = new TriageDataLocksCommand(_apprenticeshipId, TriageStatus.Restart, _fixture.Create<UserInfo>());

            //Act
            await _handler.Handle(_validCommand, default);
            await _dbContext.SaveChangesAsync();

            //Assert          
            var apprenticeshipDataLock = _confirmationDbContext.DataLocks.Where(s => s.ApprenticeshipId == apprenticeship.Id);
            apprenticeshipDataLock.Where(x => x.TriageStatus == TriageStatus.Restart).Should().HaveCount(2);            
        }

        [Test]
        public async Task Should_Not_Update_Expired_DataLocks()
        {
            //Arrange
            var apprenticeship = SetupApprenticeshipWithDatalocks(TriageStatus.Change, true);
            _validCommand = new TriageDataLocksCommand(_apprenticeshipId, TriageStatus.Restart, _fixture.Create<UserInfo>());

            //Act
            await _handler.Handle(_validCommand, default);
            await _dbContext.SaveChangesAsync();

            //Assert          
            var apprenticeshipDataLock = _confirmationDbContext.DataLocks.Where(s => s.ApprenticeshipId == apprenticeship.Id);
            apprenticeshipDataLock.Where(x => x.TriageStatus == TriageStatus.Restart).Should().HaveCount(1);
        }

        [Test]
        public async Task Should_Ignore_Passed_Datalocks()
        {
            //Arrange
            var apprenticeship = SetupApprenticeship(TriageStatus.Change, true, true);
            apprenticeship.DataLockStatus.FirstOrDefault().Status = Status.Pass;
            _validCommand = new TriageDataLocksCommand(_apprenticeshipId, TriageStatus.Restart, _fixture.Create<UserInfo>());

            //Act
            await _handler.Handle(_validCommand, default);

            //Assert
            _unitOfWorkContext.GetEvents().OfType<EntityStateChangedEvent>().Should().BeEmpty();
        }


        [Test]
        public void Should_Not_Update_If_Request_Has_Same_TriageStatus()
        {
            //Arrange
            var apprenticeship = SetupApprenticeship(TriageStatus.Change, false, false);           
            _validCommand = new TriageDataLocksCommand(_apprenticeshipId, TriageStatus.Change, _fixture.Create<UserInfo>());
            var expectedMessage = $"Trying to update data lock for apprenticeship: {_validCommand.ApprenticeshipId} with the same TriageStatus ({_validCommand.TriageStatus}) ";          

            //Act
            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _handler.Handle(_validCommand, new CancellationToken()));

            //Assert
            Assert.AreEqual(expectedMessage, exception.Message);
        }   


        [Test]
        public void Should_Not_Update_DataLock_If_Apprenticeship_HasHadSuccessful_DataLock()
        {
            //Arrange
            var apprenticeship = SetupApprenticeship(TriageStatus.Change, true, true);           
            _validCommand = new TriageDataLocksCommand(_apprenticeshipId, TriageStatus.Change, _fixture.Create<UserInfo>());
            var expectedMessage = $"Trying to update data lock for apprenticeship: {_validCommand.ApprenticeshipId} with triage status ({_validCommand.TriageStatus}) and datalock with course and price when Successful DataLock already received";

            //Act
            var exception =  Assert.ThrowsAsync<InvalidOperationException>(async () => await _handler.Handle(_validCommand, new CancellationToken()));
            
            //Assert         
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        public async Task Should_Update_DataLock_If_Apprenticeship_HasHadSuccessful_And_IsNotResolved()
        {
            //Arrange
            var apprenticeship = SetupApprenticeship(TriageStatus.Change, true, false);
            _validCommand = new TriageDataLocksCommand(_apprenticeshipId, TriageStatus.Change, _fixture.Create<UserInfo>());

            //Act
            await _handler.Handle(_validCommand, default);
            await _dbContext.SaveChangesAsync();

            //Assert          
            var apprenticeshipDataLock = _confirmationDbContext.DataLocks.Where(s => s.ApprenticeshipId == apprenticeship.Id);
            Assert.AreEqual(TriageStatus.Change, apprenticeshipDataLock.FirstOrDefault().TriageStatus);
        }

        private Apprenticeship SetupApprenticeship(TriageStatus triageStatus, bool hasHadDataLockSuccess, bool isResolved)
        {
            var fixture = new Fixture();
            var apprenticeshipId = 10082;
            var apprenticeship = new Apprenticeship
            {
                Id = apprenticeshipId,
                HasHadDataLockSuccess = hasHadDataLockSuccess,
                Cohort = new Cohort
                {
                    EmployerAccountId = fixture.Create<long>(),
                    AccountLegalEntity = new AccountLegalEntity()
                },
                DataLockStatus = new List<DataLockStatus>()
                {
                    new DataLockStatus {  
                        
                        ApprenticeshipId = apprenticeshipId,
                        DataLockEventId = 1,
                        EventStatus = EventStatus.New,
                        IsExpired = false,
                        IsResolved = isResolved,
                        TriageStatus = triageStatus,
                        ErrorCode = DataLockErrorCode.Dlock04
                    }
                },
                StartDate = DateTime.UtcNow.AddMonths(-2)
            };

            _dbContext.Apprenticeships.Add(apprenticeship);
            _dbContext.SaveChangesAsync();

            return apprenticeship;           
        }

        private Apprenticeship SetupApprenticeshipWithDatalocks(TriageStatus triageStatus, bool isExpired)
        {
            var fixture = new Fixture();
            var apprenticeshipId = 10082;
            var apprenticeship = new Apprenticeship
            {
                Id = apprenticeshipId,
                HasHadDataLockSuccess = false,
                Cohort = new Cohort
                {
                    EmployerAccountId = fixture.Create<long>(),
                    AccountLegalEntity = new AccountLegalEntity()
                },
                DataLockStatus = new List<DataLockStatus>()
                {
                    new DataLockStatus {

                        ApprenticeshipId = apprenticeshipId,
                        DataLockEventId = 1,
                        EventStatus = EventStatus.New,
                        IsExpired = isExpired,
                        TriageStatus = triageStatus,
                        ErrorCode = DataLockErrorCode.Dlock04
                    },
                    new DataLockStatus {

                        ApprenticeshipId = apprenticeshipId,
                        DataLockEventId = 2,
                        EventStatus = EventStatus.New,
                        IsExpired = false,
                        TriageStatus = triageStatus,
                        ErrorCode = DataLockErrorCode.Dlock05
                    }
                },
                StartDate = DateTime.UtcNow.AddMonths(-2)
            };

            _dbContext.Apprenticeships.Add(apprenticeship);
            _dbContext.SaveChangesAsync();

            return apprenticeship;
        }

    }
}
