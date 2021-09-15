using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Services;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public class EmailOptionalServiceTests
    {
        EmailOptionalConfiguration _config;
        IEmailOptionalService _sut;

        public EmailOptionalServiceTests()
        {
            _config = new EmailOptionalConfiguration { EmailOptionalEmployers = new long[] { 123, 456, 789 }, EmailOptionalProviders = new long[] { 321, 654, 987 } };
            _sut = new EmailOptionalService(_config);
        }

        [TestCase(123, 321)]
        [TestCase(456, 0)]
        [TestCase(0, 987)]
        public void EmailOptionalService_ApprenticeEmailIsOptionalFor_Positive(long employerId, long providerId)
        {
            Assert.IsTrue(_sut.ApprenticeEmailIsOptionalFor(employerId, providerId));
        }

        [TestCase(432, 111)]
        [TestCase(641, 0)]
        public void EmailOptionalService_ApprenticeEmailIsOptionalFor_Negative(long employerId, long providerId)
        {
            Assert.IsFalse(_sut.ApprenticeEmailIsOptionalFor(employerId, providerId));
        }

        [TestCase(456)]
        public void EmailOptionalService_ApprenticeEmailIsOptionalForEmployer_Positive(long employerId)
        {
            Assert.IsTrue(_sut.ApprenticeEmailIsOptionalForEmployer(employerId));
        }

        [TestCase(8888)]
        [TestCase(321)]
        public void EmailOptionalService_ApprenticeEmailIsOptionalForEmployer_Negative(long employerId)
        {
            Assert.IsFalse(_sut.ApprenticeEmailIsOptionalForEmployer(employerId));
        }

        [TestCase(987)]
        public void EmailOptionalService_ApprenticeEmailIsOptionalForProvider_Positive(long providerId)
        {
            Assert.IsTrue(_sut.ApprenticeEmailIsOptionalForProvider(providerId));
        }

        [TestCase(1111)]
        [TestCase(123)]
        public void EmailOptionalService_ApprenticeEmailIsOptionalForProvider_Negative(long providerId)
        {
            Assert.IsFalse(_sut.ApprenticeEmailIsOptionalForProvider(providerId));
        }

        [TestCase(111, 222)]
        [TestCase(96167, 888)]
        [TestCase(0, 456)]
        [TestCase(987, 0)]
        public void EmailOptionalService_ApprenticeEmailIsRequiredFor_Positive(long employerId, long providerId)
        {
            Assert.IsTrue(_sut.ApprenticeEmailIsRequiredFor(employerId, providerId));
        }

        [TestCase(123, 321)]
        [TestCase(456, 0)]
        [TestCase(0, 987)]
        public void EmailOptionalService_ApprenticeEmailIsRequiredFor_Negative(long employerId, long providerId)
        {
            var res = _sut.ApprenticeEmailIsRequiredFor(employerId, providerId);

            Assert.IsFalse(_sut.ApprenticeEmailIsRequiredFor(employerId, providerId));
        }

        [TestCase(321)]
        [TestCase(444999)]
        public void EmailOptionalService_ApprenticeEmailIsRequiredForEmployer_Positive(long employerId)
        {
            Assert.IsTrue(_sut.ApprenticeEmailIsRequiredForEmployer(employerId));
        }

        [TestCase(123)]        
        public void EmailOptionalService_ApprenticeEmailIsRequiredForEmployer_Negative(long employerId)
        {
            Assert.False(_sut.ApprenticeEmailIsRequiredForEmployer(employerId));
        }

        [TestCase(123)]
        [TestCase(7777)]
        public void EmailOptionalService_ApprenticeEmailIsRequiredForProvider_Positive(long providerId)
        {
            Assert.IsTrue(_sut.ApprenticeEmailIsRequiredForProvider(providerId));
        }

        [TestCase(654)]
        public void EmailOptionalService_ApprenticeEmailIsRequiredForProvider_Negative(long providerId)
        {
            Assert.IsFalse(_sut.ApprenticeEmailIsRequiredForProvider(providerId));
        }

        [TestCase(654, 1234)]
        public void EmailOptionalService_ApprenticeEmailIsRequiredForProviderOrEmployer_OnNullList(long providerId, long employerId)
        {
            var config = new EmailOptionalConfiguration { EmailOptionalEmployers = null, EmailOptionalProviders = null };
            var sut = new EmailOptionalService(config);
            Assert.IsFalse(sut.ApprenticeEmailIsOptionalFor(employerId, providerId));
        }

        [TestCase(654)]
        public void EmailOptionalService_ApprenticeEmailIsRequiredForProvider_OnNullList(long providerId)
        {
            var config = new EmailOptionalConfiguration { EmailOptionalEmployers = null, EmailOptionalProviders = null };
            var sut = new EmailOptionalService(config);

            Assert.IsFalse(sut.ApprenticeEmailIsOptionalForProvider(providerId));
        }

        [TestCase(1234)]
        public void EmailOptionalService_ApprenticeEmailIsRequiredEmployer_OnNullList(long employerId)
        {
            var config = new EmailOptionalConfiguration { EmailOptionalEmployers = null, EmailOptionalProviders = null };
            var sut = new EmailOptionalService(config);

            Assert.IsFalse(sut.ApprenticeEmailIsOptionalForEmployer(employerId));
        }
    }
}
