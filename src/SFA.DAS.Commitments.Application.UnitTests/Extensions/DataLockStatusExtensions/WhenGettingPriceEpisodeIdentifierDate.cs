using System;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Extensions;

namespace SFA.DAS.Commitments.Application.UnitTests.Extensions.DataLockStatusExtensions
{
    [TestFixture]
    public class WhenGettingPriceEpisodeIdentifierDate
    {
        [TestCase("3-473-1-03/05/2017","2017-05-03")]
        [TestCase("123-21/05/2017", "2017-05-21")]
        public void ThenTheDateIsParsedCorrectly(string priceEpisodeIdentifer, DateTime expectedDate)
        {
            var datalock = new DataLockStatus
            {
                PriceEpisodeIdentifier = priceEpisodeIdentifer
            };

            Assert.AreEqual(expectedDate, datalock.GetDateFromPriceEpisodeIdentifier());
        }
    }
}
