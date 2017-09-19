﻿using System;
using System.Linq;

using FluentAssertions;
using NUnit.Framework;

using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Provider.Events.Api.Types;

using DataLockEventError = SFA.DAS.Provider.Events.Api.Types.DataLockEventError;

namespace SFA.DAS.Commitments.Infrastructure.UnitTests.Services.PaymentEventMapper
{
    [TestFixture]
    public class WhenMappingPaymentEvent
    {
        private Infrastructure.Services.PaymentEventMapper _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new Infrastructure.Services.PaymentEventMapper();
        }

        [Test]
        public void ThenDoingASimpleMapping()
        {
            var processDateTime = new DateTime(1998, 12, 08);
            var ilrStartDate = new DateTime(2020, 12, 08);
            var lrPriceEffectiveFromDate = new DateTime(2020, 12, 12);
            var lrPriceEffectiveToDate = new DateTime(2022, 12, 24);
            var result = _sut.Map(
                new DataLockEvent
                    {
                        Id = 5L,
                        ProcessDateTime = processDateTime,
                        PriceEpisodeIdentifier = "price-episode-identifier",
                        ApprenticeshipId = 12399L,
                        IlrStartDate = ilrStartDate,
                        IlrPriceEffectiveFromDate = lrPriceEffectiveFromDate,
                        IlrPriceEffectiveToDate = lrPriceEffectiveToDate,
                        IlrTrainingPrice = 1600,
                        IlrEndpointAssessorPrice = 500,
                        Errors = new[]
                                     {
                                         new DataLockEventError { ErrorCode = "DLOCK_04", SystemDescription = "No matching record found in the employer digital account for the framework code" },
                                         new DataLockEventError { ErrorCode = "DLOCK_05", SystemDescription = "No matching record found in the employer digital account for the programme type" }
                                     }

                });

            result.DataLockEventId.Should().Be(5L);
            result.DataLockEventDatetime.Should().Be(processDateTime);
            result.PriceEpisodeIdentifier.Should().Be("price-episode-identifier");
            result.ApprenticeshipId.Should().Be(12399L);
            result.IlrActualStartDate.Should().Be(ilrStartDate);
            result.IlrEffectiveFromDate.Should().Be(lrPriceEffectiveFromDate);
            result.IlrPriceEffectiveToDate.Should().Be(lrPriceEffectiveToDate);
            result.IlrTotalCost.Should().Be(2100M);

            result.Status.Should().Be(Status.Fail);
            result.TriageStatus.Should().Be(TriageStatus.Unknown);
        }

        [Test]
        public void ThenErrorCodesShouldBeMerged()
        {
            var errorCodes = new[]
            {
                new DataLockEventError { ErrorCode = "Dlock01" },
                new DataLockEventError { ErrorCode = "Dlock02" }
            };

            var result = _sut.Map(
                new DataLockEvent
                {
                    Errors = errorCodes
                });

            result.ErrorCode.Should().Be((DataLockErrorCode)3);
        }

        [Test]
        public void ThenErrorCodesShouldIgnoreValuesThatAreNotRecognised()
        {
            var errorCodes = new[]
            {
                new DataLockEventError { ErrorCode = "Dlock01" },
                new DataLockEventError { ErrorCode = "Abba213" }
            };

            var result = _sut.Map(
                new DataLockEvent
                {
                    Errors = errorCodes
                });

            result.ErrorCode.Should().Be((DataLockErrorCode)1);
        }

        [Test]
        public void ThenErrorCodesShouldIgnoreCaseAndUnderscore()
        {
            var errorCodes = new[]
            {
                new DataLockEventError { ErrorCode = "DLOCK_04" },
                new DataLockEventError { ErrorCode = "DLOCK_05" }
            };

            var result = _sut.Map(
                new DataLockEvent
                {
                    Errors = errorCodes
                });

            result.ErrorCode.Should().Be((DataLockErrorCode)24);
        }

        [Test]
        public void ThenErrorCodesShouldBeNone()
        {
            var errorCodes = new DataLockEventError[0];

            var result = _sut.Map(
                new DataLockEvent
                {
                    Errors = errorCodes
                });

            result.ErrorCode.Should().Be(DataLockErrorCode.None);
        }

        [TestCase(12, 25, "12")]
        [TestCase(2, 25, "2")]
        public void ThenMappingStandards(int? standardCode, int? programType, string expectedTrainingCode)
        {
            var result = _sut.Map(
                new DataLockEvent
                    {
                        IlrStandardCode = standardCode,
                        IlrProgrammeType = programType,
                        Errors = new DataLockEventError[0]
                });

            result.IlrTrainingCourseCode.Should().Be(expectedTrainingCode);
            result.IlrTrainingType.Should().Be(TrainingType.Standard);
        }

        [TestCase(200, 21, 4, "200-21-4")]
        [TestCase(403, 2, 1, "403-2-1")]
        public void ThenMappingFramework(int? frameworkCode, int? progType, int? pathwayCode, string exprectedId)
        {
            var result = _sut.Map(
                new DataLockEvent
                    {
                        IlrFrameworkCode = frameworkCode,
                        IlrProgrammeType = progType,
                        IlrPathwayCode = pathwayCode,
                        Errors = new DataLockEventError[0]
                    });

            result.IlrTrainingCourseCode.Should().Be(exprectedId);
            result.IlrTrainingType.Should().Be(TrainingType.Framework);
        }

        [Test]
        public void ShouldWorkWithDataLocksNull()
        {
            var result = _sut.Map(
                new DataLockEvent
                {
                    IlrFrameworkCode = 129,
                    IlrProgrammeType = 2,
                    IlrPathwayCode = 12,
                    Errors = null,
                });

            result.IlrTrainingCourseCode.Should().Be("129-2-12");
            result.IlrTrainingType.Should().Be(TrainingType.Framework);
            result.ErrorCode.Should().Be(DataLockErrorCode.None);
            result.Status.Should().Be(Status.Pass);
        }
    }
}