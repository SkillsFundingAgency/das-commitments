﻿using AutoFixture;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    public class BulkUploadValidateCommandHandlerTestsFixture
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        public IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse> Handler { get; set; }
        public Mock<IOverlapCheckService> OverlapCheckService { get; set; }
        public Mock<IAcademicYearDateProvider> AcademicYearDateProvider { get; set; }
        public List<CsvRecord> CsvRecords { get; set; }
        public BulkUploadValidateCommand Command { get; set; }

        public OverlapCheckResult OverlapCheckResult { get; set; }
        public EmailOverlapCheckResult EmailOverlapCheckResult { get; set; }

        public BulkUploadValidateCommandHandlerTestsFixture()
        {
            CsvRecords = new List<CsvRecord>();
            PopulateCsvRecord();
            Command = new BulkUploadValidateCommand()
            {
                CsvRecords = CsvRecords,
                ProviderId = 1
            };

            OverlapCheckService = new Mock<IOverlapCheckService>();
            OverlapCheckResult =  new OverlapCheckResult(false, false);

            OverlapCheckService.Setup(x => x.CheckForOverlaps(It.IsAny<string>(), It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(), null, CancellationToken.None))
                .ReturnsAsync(() => OverlapCheckResult);

            EmailOverlapCheckResult = new EmailOverlapCheckResult(1, OverlapStatus.None, false);
            OverlapCheckService.Setup(x => x.CheckForEmailOverlaps(It.IsAny<string>(), It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(), null, null, CancellationToken.None))
                .ReturnsAsync(() => EmailOverlapCheckResult);

            AcademicYearDateProvider = new Mock<IAcademicYearDateProvider>();
            AcademicYearDateProvider.Setup(x => x.CurrentAcademicYearEndDate).Returns(DateTime.Parse(CsvRecords[0].StartDate));

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                 .Options);
            SetupDbData();
            Handler = new BulkUploadValidateCommandHandler(Mock.Of<ILogger<BulkUploadValidateCommandHandler>>()
                , new Lazy<ProviderCommitmentsDbContext>(() => Db)
                , OverlapCheckService.Object
                , AcademicYearDateProvider.Object
                );
        }

        private void SetupDbData()
        {
            var fixture = new Fixture();

            var account = new Account()
                .Set(a => a.Name, "Employer1")
                .Set(a => a.LevyStatus, ApprenticeshipEmployerType.Levy);

            var ale = new AccountLegalEntity()
            .Set(al => al.PublicHashedId, "XEGE5X")
            .Set(al => al.Account, account);

            var Cohort = new Cohort()
            .Set(c => c.Id, 111)
            .Set(c => c.EmployerAccountId, 222)
            .Set(c => c.ProviderId, 333)
            .Set(c => c.Reference, "P97BKL")
            .Set(c => c.WithParty, Party.Provider)
            .Set(c => c.AccountLegalEntity, ale);

            var standard = new Standard()
                .Set(x => x.LarsCode, 59)
                .Set(x => x.StandardUId, Guid.NewGuid().ToString())
                .Set(x => x.EffectiveFrom, new DateTime(2000, 1, 1))
                .Set(x => x.EffectiveTo, new DateTime(2050, 1, 1));

            Db.Cohorts.Add(Cohort);
            Db.Standards.Add(standard);
            Db.SaveChanges();

        }

        internal async Task<Standard> GetStandard()
        {
            var standard = await Db.Standards.FirstOrDefaultAsync();
            return standard;
        }

        private void PopulateCsvRecord()
        {
            CsvRecords.Add(new CsvRecord
            {
                RowNumber = 1,
                AgreementId = "XEGE5X",
                CohortRef = "P97BKL",
                ULN = "6591690157",
                FamilyName = "Smith",
                GivenNames = "Mark",
                DateOfBirth = "2000-01-02",
                StdCode = "59",
                StartDate = "2019-05-01",
                EndDate = "2020-05",
                TotalPrice = "2000",
                EPAOrgID = "EPA0001",
                ProviderRef = "ZB88",
                EmailAddress = "abc34628125987@abc.com"
            });
        }

        internal void SetUpDuplicateUln()
        {
            CsvRecords.Add(new CsvRecord
            {
                RowNumber = 2,
                AgreementId = "XEGE5X",
                CohortRef = "P97BKL",
                ULN = "6591690157",
                FamilyName = "Smith2",
                GivenNames = "Mark2",
                DateOfBirth = "2002-01-02",
                StdCode = "59",
                StartDate = "2019-05-01",
                EndDate = "2020-05",
                TotalPrice = "2000",
                EPAOrgID = "EPA0001",
                ProviderRef = "ZB88",
                EmailAddress = "abc34628125987@abc2.com"
            });
        }

        internal void SetUpDuplicateEmail()
        {
            CsvRecords.Add(new CsvRecord
            {
                RowNumber = 2,
                AgreementId = "XEGE5X",
                CohortRef = "P97BKL",
                ULN = "6591690158",
                FamilyName = "Smith2",
                GivenNames = "Mark2",
                DateOfBirth = "2002-01-02",
                StdCode = "59",
                StartDate = "2019-05-01",
                EndDate = "2020-05",
                TotalPrice = "2000",
                EPAOrgID = "EPA0001",
                ProviderRef = "ZB88",
                EmailAddress = "abc34628125987@abc.com"
            });
        }

        public async Task<BulkUploadValidateApiResponse> Handle()
        {
            return await Handler.Handle(Command, CancellationToken.None);
        }

        public void ValidateError(BulkUploadValidateApiResponse errors, int numberOfErrors, string property, string errorText)
        {
            Assert.AreEqual(numberOfErrors, errors.BulkUploadValidationErrors.Count);
            Assert.AreEqual(numberOfErrors, errors.BulkUploadValidationErrors[0].Errors.Count);
            Assert.AreEqual(errorText, errors.BulkUploadValidationErrors[0].Errors[0].ErrorText);
            Assert.AreEqual(property, errors.BulkUploadValidationErrors[0].Errors[0].Property);
        }

        internal BulkUploadValidateCommandHandlerTestsFixture SetCohortRef(string cohortRef)
        {
            CsvRecords[0].CohortRef = cohortRef;
            return this;
        }

        internal BulkUploadValidateCommandHandlerTestsFixture SetAgreementId(string agreementId)
        {
            CsvRecords[0].AgreementId = agreementId;
            return this;
        }

        internal BulkUploadValidateCommandHandlerTestsFixture SetProviderRef(string providerRef)
        {
            CsvRecords[0].ProviderRef = providerRef;
            return this;
        }

        internal BulkUploadValidateCommandHandlerTestsFixture SetStartDate(string startDate)
        {
            CsvRecords[0].StartDate = startDate;
            return this;
        }

        internal BulkUploadValidateCommandHandlerTestsFixture SetEndDate(string endDate)
        {
            CsvRecords[0].EndDate = endDate;
            return this;
        }

        internal BulkUploadValidateCommandHandlerTestsFixture SetAfterAcademicYearEndDate()
        {
            AcademicYearDateProvider.Setup(x => x.CurrentAcademicYearEndDate).Returns(DateTime.Parse(CsvRecords[0].StartDate).AddYears(-1).AddDays(-1));
            return this;
        }

        internal BulkUploadValidateCommandHandlerTestsFixture SetCourseEffectiveFromAfterCourseStartDate()
        {
            var standard = Db.Standards.FirstOrDefaultAsync().Result;
            standard.EffectiveFrom = DateTime.Parse(CsvRecords[0].StartDate).AddDays(1);
            Db.SaveChanges();
            return this;
        }

        internal BulkUploadValidateCommandHandlerTestsFixture SetCourseEffectiveToBeforeCourseStartDate()
        {
            var standard = Db.Standards.FirstOrDefaultAsync().Result;
            standard.EffectiveTo = DateTime.Parse(CsvRecords[0].StartDate).AddDays(-1);
            Db.SaveChanges();
            return this;
        }

        internal BulkUploadValidateCommandHandlerTestsFixture SetIsTransferFunded()
        {
            var cohort = Db.Cohorts.FirstOrDefaultAsync().Result;
            cohort.TransferSenderId = 1;
            Db.SaveChanges();
            return this;
        }

        internal BulkUploadValidateCommandHandlerTestsFixture SetStdCode(string stdCode)
        {
            CsvRecords[0].StdCode = stdCode;
            return this;
        }

        internal BulkUploadValidateCommandHandlerTestsFixture SetGivenNames(string givenName)
        {
            CsvRecords[0].GivenNames = givenName;
            return this;
        }

        internal BulkUploadValidateCommandHandlerTestsFixture SetFamilyName(string familyName)
        {
            CsvRecords[0].FamilyName = familyName;
            return this;
        }

        internal BulkUploadValidateCommandHandlerTestsFixture SetEmailAddress(string emailAddress)
        {
            CsvRecords[0].EmailAddress = emailAddress;
            return this;
        }

        internal BulkUploadValidateCommandHandlerTestsFixture SetDateOfBirth(string dateOfBirth)
        {
            CsvRecords[0].DateOfBirth = dateOfBirth;
            return this;
        }

        internal BulkUploadValidateCommandHandlerTestsFixture SetUln(string uln)
        {
            CsvRecords[0].ULN = uln;
            return this;
        }

        internal BulkUploadValidateCommandHandlerTestsFixture SetTotalPrice(string totalPrice)
        {
            CsvRecords[0].TotalPrice = totalPrice;
            return this;
        }

        internal BulkUploadValidateCommandHandlerTestsFixture SetWithParty(Party withParty)
        {
            Db.Cohorts.FirstOrDefaultAsync().Result.WithParty = withParty;
            Db.SaveChanges();
            return this;
        }

        internal void SetOverlappingDate(bool startDate, bool endDate)
        {
            OverlapCheckResult = new OverlapCheckResult(startDate, endDate);
        }

        internal void SetOverlappingEmail(OverlapStatus status)
        {
            EmailOverlapCheckResult = new EmailOverlapCheckResult(1, status, true);
        }

        internal BulkUploadValidateCommandHandlerTestsFixture SetChangeOfParty()
        {
            var account = new Account()
               .Set(a => a.Id, 20)
               .Set(a => a.Name, "Employer2")
               .Set(a => a.LevyStatus, ApprenticeshipEmployerType.Levy);

            var ale = new AccountLegalEntity()
            .Set(a => a.Id, 20)
            .Set(al => al.PublicHashedId, "XEGE5Y")
            .Set(al => al.Account, account);

            Db.Accounts.Add(account);
            Db.SaveChanges();

            var ChangeOfPartyCohort = new Cohort()
            .Set(a => a.Id, 20)
            .Set(x => x.ProviderId, 999)
            .Set(c => c.Reference, "P97BKM")
            .Set(c => c.WithParty, Party.Provider)
            .Set(c => c.AccountLegalEntity, ale);

            Db.Cohorts.Add(ChangeOfPartyCohort);
            Db.SaveChanges();

            var fixture = new Fixture();
            var cohort =  Db.Cohorts.FirstOrDefaultAsync().Result;
            var apprenticeship = new Apprenticeship();
        
            apprenticeship.Set(x => x.FirstName, "FirstName");
            apprenticeship.Set(x => x.LastName, "LastName");
            apprenticeship.Set(x => x.Email, "abc@hotmail.com");
            apprenticeship.Set(x => x.EmailAddressConfirmed, true);
            apprenticeship.Set(x => x.Cohort, ChangeOfPartyCohort);

            Db.Apprenticeships.Add(apprenticeship);

            var request = new ChangeOfPartyRequest();

            request.Set(x => x.Apprenticeship, apprenticeship);
            request.Set(x => x.ChangeOfPartyType, fixture.Create<ChangeOfPartyRequestType>());
            request.Set(x => x.OriginatingParty, fixture.Create<Party>());
            request.Set(x => x.Status, fixture.Create<ChangeOfPartyRequestStatus>());
            request.Set(x => x.AccountLegalEntity, cohort.AccountLegalEntity);
            request.Set(x => x.StartDate, fixture.Create<DateTime>());
            request.Set(x => x.EndDate, fixture.Create<DateTime>());

            cohort.ChangeOfPartyRequest = request;
            cohort.ChangeOfPartyRequestId = request.Id;
            Db.SaveChanges();
            return this;
        }

        private DateTime? GetValidDate(string date, string format)
        {
            DateTime outDateTime;
            if (!string.IsNullOrWhiteSpace(date) &&
                DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out outDateTime))
                return outDateTime;
            return null;
        }
    }
}