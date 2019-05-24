using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Castle.Core.Internal;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Mapping.Reservations;
using SFA.DAS.Reservations.Api.Client.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.Reservations
{
    [TestFixture]
    public class ValidationResultToReservationValidationErrorMapperTests
    {
        private Fixture _autoFixture;
        private ValidationResult _source;
        private ValidationResultToReservationValidationErrorMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            _autoFixture = new Fixture();
            _source = _autoFixture.Create<ValidationResult>();

            _mapper = new ValidationResultToReservationValidationErrorMapper();
        }

        [Test]
        public async Task Map_Errors_ShouldBeSet()
        {
            var result = await _mapper.Map(_source);

            Assert.AreEqual(_source.ValidationErrors.Length, result.ValidationErrors.Length);
            foreach (var error in result.ValidationErrors)
            {
                Assert.IsTrue(result.ValidationErrors.Any(x =>
                    x.Code == error.Code && x.PropertyName == error.PropertyName && x.Reason == error.Reason));
            }
        }
    }
}
