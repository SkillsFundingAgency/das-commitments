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
            Assert.That(_sut.ApprenticeEmailIsOptionalFor(employerId, providerId), Is.True);
        }

        [TestCase(432, 111)]
        [TestCase(641, 0)]
        public void EmailOptionalService_ApprenticeEmailIsOptionalFor_Negative(long employerId, long providerId)
        {
            Assert.That(_sut.ApprenticeEmailIsOptionalFor(employerId, providerId), Is.False);
        }

        [TestCase(456)]
        public void EmailOptionalService_ApprenticeEmailIsOptionalForEmployer_Positive(long employerId)
        {
            Assert.That(_sut.ApprenticeEmailIsOptionalForEmployer(employerId), Is.True);
        }

        [TestCase(8888)]
        [TestCase(321)]
        public void EmailOptionalService_ApprenticeEmailIsOptionalForEmployer_Negative(long employerId)
        {
            Assert.That(_sut.ApprenticeEmailIsOptionalForEmployer(employerId), Is.False);
        }

        [TestCase(987)]
        public void EmailOptionalService_ApprenticeEmailIsOptionalForProvider_Positive(long providerId)
        {
            Assert.That(_sut.ApprenticeEmailIsOptionalForProvider(providerId), Is.True);
        }

        [TestCase(1111)]
        [TestCase(123)]
        public void EmailOptionalService_ApprenticeEmailIsOptionalForProvider_Negative(long providerId)
        {
            Assert.That(_sut.ApprenticeEmailIsOptionalForProvider(providerId), Is.False);
        }

        [TestCase(111, 222)]
        [TestCase(96167, 888)]
        [TestCase(0, 456)]
        [TestCase(987, 0)]
        public void EmailOptionalService_ApprenticeEmailIsRequiredFor_Positive(long employerId, long providerId)
        {
            Assert.That(_sut.ApprenticeEmailIsRequiredFor(employerId, providerId), Is.True);
        }

        [TestCase(123, 321)]
        [TestCase(456, 0)]
        [TestCase(0, 987)]
        public void EmailOptionalService_ApprenticeEmailIsRequiredFor_Negative(long employerId, long providerId)
        {
            var res = _sut.ApprenticeEmailIsRequiredFor(employerId, providerId);

            Assert.That(_sut.ApprenticeEmailIsRequiredFor(employerId, providerId), Is.False);
        }

        [TestCase(321)]
        [TestCase(444999)]
        public void EmailOptionalService_ApprenticeEmailIsRequiredForEmployer_Positive(long employerId)
        {
            Assert.That(_sut.ApprenticeEmailIsRequiredForEmployer(employerId), Is.True);
        }

        [TestCase(123)]        
        public void EmailOptionalService_ApprenticeEmailIsRequiredForEmployer_Negative(long employerId)
        {
            Assert.That(_sut.ApprenticeEmailIsRequiredForEmployer(employerId), Is.False);
        }

        [TestCase(123)]
        [TestCase(7777)]
        public void EmailOptionalService_ApprenticeEmailIsRequiredForProvider_Positive(long providerId)
        {
            Assert.That(_sut.ApprenticeEmailIsRequiredForProvider(providerId), Is.True);
        }

        [TestCase(654)]
        public void EmailOptionalService_ApprenticeEmailIsRequiredForProvider_Negative(long providerId)
        {
            Assert.That(_sut.ApprenticeEmailIsRequiredForProvider(providerId), Is.False);
        }

        [TestCase(654, 1234)]
        public void EmailOptionalService_ApprenticeEmailIsRequiredForProviderOrEmployer_OnNullList(long providerId, long employerId)
        {
            var config = new EmailOptionalConfiguration { EmailOptionalEmployers = null, EmailOptionalProviders = null };
            var sut = new EmailOptionalService(config);

            Assert.That(sut.ApprenticeEmailIsOptionalFor(employerId, providerId), Is.False);
        }

        [TestCase(654)]
        public void EmailOptionalService_ApprenticeEmailIsRequiredForProvider_OnNullList(long providerId)
        {
            var config = new EmailOptionalConfiguration { EmailOptionalEmployers = null, EmailOptionalProviders = null };
            var sut = new EmailOptionalService(config);

            Assert.That(sut.ApprenticeEmailIsOptionalForProvider(providerId), Is.False);
        }

        [TestCase(1234)]
        public void EmailOptionalService_ApprenticeEmailIsRequiredEmployer_OnNullList(long employerId)
        {
            var config = new EmailOptionalConfiguration { EmailOptionalEmployers = null, EmailOptionalProviders = null };
            var sut = new EmailOptionalService(config);

            Assert.That(sut.ApprenticeEmailIsOptionalForEmployer(employerId), Is.False);
        }
    }
}
