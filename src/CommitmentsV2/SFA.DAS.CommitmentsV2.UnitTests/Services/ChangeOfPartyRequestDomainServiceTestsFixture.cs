﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.ProviderRelationships.Api.Client;
using SFA.DAS.ProviderRelationships.Types.Dtos;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    public class ChangeOfPartyRequestDomainServiceTestsFixture
    {
        private readonly ChangeOfPartyRequestDomainService _domainService;
        private readonly Fixture Fixture = new Fixture();
        public ProviderCommitmentsDbContext Db { get; private set; }
        public Exception Exception { get; private set; }
        public Mock<IAuthenticationService> AuthenticationService { get; }
        public Mock<ICurrentDateTime> CurrentDateTime { get; }
        public DateTime Now { get; }
        public Mock<IProviderRelationshipsApiClient> ProviderRelationshipsApiClient { get; }
        public Mock<Apprenticeship> Apprenticeship { get; private set; }
        public Cohort Cohort { get; private set; }

        public ChangeOfPartyRequestType ChangeOfPartyRequestType { get; private set; }
        public Party OriginatingParty { get; private set; }
        public long ApprenticeshipId { get; private set; }
        public long NewPartyId { get; private set; }
        public int? Price { get; private set; }
        public DateTime? StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public UserInfo UserInfo { get; private set; }

        public ChangeOfPartyRequest ApprenticeshipChangeOfPartyRequestResult { get; private set; }
        public ChangeOfPartyRequest Result { get; private set; }

        public ChangeOfPartyRequestDomainServiceTestsFixture(Party party, ChangeOfPartyRequestType changeOfPartyRequestType)
        {
            Now = DateTime.UtcNow;
            var uow = new UnitOfWorkContext();

            CurrentDateTime = new Mock<ICurrentDateTime>();
            CurrentDateTime.Setup(d => d.UtcNow).Returns(Now);

            ProviderRelationshipsApiClient = new Mock<IProviderRelationshipsApiClient>();
            ProviderRelationshipsApiClient.Setup(x =>
                    x.GetAccountProviderLegalEntitiesWithPermission(
                        It.IsAny<GetAccountProviderLegalEntitiesWithPermissionRequest>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new GetAccountProviderLegalEntitiesWithPermissionResponse
                {
                    AccountProviderLegalEntities = new List<AccountProviderLegalEntityDto>
                    {
                            new AccountProviderLegalEntityDto {AccountLegalEntityId = NewPartyId}
                    }
                });

            AuthenticationService = new Mock<IAuthenticationService>();
            AuthenticationService.Setup(x => x.GetUserParty()).Returns(OriginatingParty);

            SetupTestData();

            OriginatingParty = party;
            ChangeOfPartyRequestType = changeOfPartyRequestType;            
            NewPartyId = Fixture.Create<long>();
            Price = Fixture.Create<int?>();
            StartDate = Fixture.Create<DateTime?>();
            EndDate = Fixture.Create<DateTime?>();
            UserInfo = Fixture.Create<UserInfo>();

            _domainService = new ChangeOfPartyRequestDomainService(
                new Lazy<ProviderCommitmentsDbContext>(() => Db),
                AuthenticationService.Object,
                CurrentDateTime.Object,
                ProviderRelationshipsApiClient.Object);
        }

        private void SetupTestData()
        {
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                .EnableSensitiveDataLogging()
                .Options);

            ApprenticeshipId = Fixture.Create<long>();
            ApprenticeshipChangeOfPartyRequestResult = Fixture.Create<ChangeOfPartyRequest>();
            Cohort = new Cohort
            {
                Id = 1000,
                ProviderId = Fixture.Create<long>(),
                AccountLegalEntity = new AccountLegalEntity()
            };
            Apprenticeship = new Mock<Apprenticeship>();
            Apprenticeship.Setup(x => x.Id).Returns(ApprenticeshipId);
            Apprenticeship.Setup(x => x.CommitmentId).Returns(1000);
            Apprenticeship.Setup(x => x.Cohort).Returns(Cohort);
            Apprenticeship.Setup(x => x.CreateChangeOfPartyRequest(It.IsAny<ChangeOfPartyRequestType>(),
                    It.IsAny<Party>(), It.IsAny<long>(), It.IsAny<int?>(), It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(), It.IsAny<UserInfo>(), It.IsAny<DateTime>()))
                .Returns(ApprenticeshipChangeOfPartyRequestResult);

            Db.Apprenticeships.Add(Apprenticeship.Object);
            Db.Cohorts.Add(Cohort);
            Db.SaveChanges();
        }

        public ChangeOfPartyRequestDomainServiceTestsFixture WithOriginatingParty(Party party)
        {
            OriginatingParty = party;
            return this;
        }

        public ChangeOfPartyRequestDomainServiceTestsFixture WithChangeOfPartyRequestType(ChangeOfPartyRequestType requestType)
        {
            ChangeOfPartyRequestType = requestType;
            return this;
        }

        public ChangeOfPartyRequestDomainServiceTestsFixture WithNoProviderPermission()
        {
            ProviderRelationshipsApiClient.Setup(x =>
                    x.GetAccountProviderLegalEntitiesWithPermission(
                        It.IsAny<GetAccountProviderLegalEntitiesWithPermissionRequest>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new GetAccountProviderLegalEntitiesWithPermissionResponse
                {
                    AccountProviderLegalEntities = new List<AccountProviderLegalEntityDto>()
                });

            return this;
        }

        public ChangeOfPartyRequestDomainServiceTestsFixture WithSameTrainingProviderWhenRequestingChangeOfProvider()
        {
            ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeProvider;
            NewPartyId = Cohort.ProviderId;

            return this;
        }

        public async Task CreateChangeOfPartyRequest()
        {
            AuthenticationService.Setup(x => x.GetUserParty()).Returns(OriginatingParty);

            try
            {
                Result = await _domainService.CreateChangeOfPartyRequest(ApprenticeshipId,
                    ChangeOfPartyRequestType, NewPartyId, Price, StartDate, EndDate, UserInfo, new CancellationToken());

                Db.SaveChanges();
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
        }

        public void VerifyAggregateMethodInvoked()
        {
            Apprenticeship.Verify(x =>
                x.CreateChangeOfPartyRequest(
                    It.Is<ChangeOfPartyRequestType>(t => t == ChangeOfPartyRequestType),
                    It.Is<Party>(p => p == OriginatingParty),
                    It.Is<long>(id => id == NewPartyId),
                    It.Is<int?>(p => p == Price),
                    It.Is<DateTime?>(s => s == StartDate),
                    It.Is<DateTime?>(e => e == EndDate),
                    It.Is<UserInfo>(u => u == UserInfo),
                    It.Is<DateTime>(n => n == Now))
                , Times.Once);
        }

        public void VerifyResult()
        {
            Assert.IsNull(Exception);
            Assert.IsNotNull(Result);
            Assert.AreEqual(ApprenticeshipChangeOfPartyRequestResult, Result);
        }

        public void VerifyResultAddedToDbContext()
        {
            Assert.Contains(Result, Db.ChangeOfPartyRequests.ToList());
        }

        public void VerifyException<T>()
        {
            Assert.IsNotNull(Exception);
            Assert.IsInstanceOf<T>(Exception);
        }
    }
}
