using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Services;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.Services.ModelMapperTests
{
    [TestFixture]
    public class WhenIMap
    {
        private TestMappingSource _source;
        private TestMappingDestination _destination;

        private Mock<IMapper<TestMappingSource, TestMappingDestination>> _mockMapper;
        private Mock<IServiceProvider> _mockServiceProvider;

        private ModelMapper _modelMapper;

        [SetUp]
        public void Arrange()
        {
            _source = new TestMappingSource();
            _destination = new TestMappingDestination();

            _mockMapper = new Mock<IMapper<TestMappingSource, TestMappingDestination>>();
            _mockMapper.Setup(x => x.Map(It.IsAny<TestMappingSource>())).Returns(Task.FromResult(_destination));

            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns(_mockMapper.Object);

            _modelMapper = new ModelMapper(_mockServiceProvider.Object);
        }

        [Test]
        public void Then_The_Correct_Mapper_Is_Retrieved_From_ServiceProvider()
        {
            _modelMapper.Map<TestMappingDestination>(_source);
            var expectedTargetMapper = typeof(IMapper<TestMappingSource, TestMappingDestination>);
            _mockServiceProvider.Verify(x => x.GetService(It.Is<Type>(type => type == expectedTargetMapper)));
        }

        [Test]
        public void Then_Mapper_Map_Is_Called()
        {
            _modelMapper.Map<TestMappingDestination>(_source);
            _mockMapper.Verify(x => x.Map(It.Is<TestMappingSource>(s => s == _source)), Times.Once);
        }

        [Test]
        public void Throws_If_No_Mapper_Found()
        {
            _mockServiceProvider.Reset();
            var exception = Assert.Throws<InvalidOperationException>(() => _modelMapper.Map<TestMappingDestination>(_source));

            var expectedMessage = $"Unable to locate implementation of IMapper<{nameof(TestMappingSource)},{nameof(TestMappingDestination)}>";
            Assert.That(exception.Message, Is.EqualTo(expectedMessage));
        }

        public class TestMappingSource
        {
        }

        public class TestMappingDestination
        {
        }
    }
}
