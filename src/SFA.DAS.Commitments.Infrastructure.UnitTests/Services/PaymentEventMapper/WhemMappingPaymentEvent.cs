using System;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

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
            var lrPriceEffectiveDate = new DateTime(2020, 12, 12);
            var result = _sut.Map(
                new DataLockEvent
                    {
                        Id = 5L,
                        ProcessDateTime = processDateTime,
                        PriceEpisodeIdentifier = "price-episode-identifier",
                        ApprenticeshipId = 12399L,
                        IlrStartDate = ilrStartDate,
                        IlrPriceEffectiveDate = new DateTime(2020, 12, 12),
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
            result.IlrEffectiveFromDate.Should().Be(lrPriceEffectiveDate);
            result.IlrTotalCost.Should().Be(2100M);

            result.ErrorCodes.Count().Should().Be(2);

            result.DataLockStatus.Should().Be(DataLockStatus.Fail);
            result.TriageStatus.Should().Be(TriageStatus.None);

        }

    }
}