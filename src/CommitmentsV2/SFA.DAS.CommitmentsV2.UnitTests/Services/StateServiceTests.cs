namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public class StateServiceTests
    {
        private WhenGettingStateTestFixture _fixture;
        
        [SetUp]
        public void Arrange()
        {
            _fixture = new WhenGettingStateTestFixture();
        }

        [Test]
        public void PublicInstancePropertiesAreCaptured()
        {
            var result = _fixture.GetState();
            Assert.Multiple(() =>
            {
                Assert.That(result.ContainsKey(nameof(GetStateTestClass.TestPublicProperty)), Is.True);
                Assert.That(result[nameof(GetStateTestClass.TestPublicProperty)], Is.EqualTo(_fixture.TestObject.TestPublicProperty));
            });
        }

        [Test]
        public void PrivateInstancePropertiesAreCaptured()
        {
            var result = _fixture.GetState();
            Assert.Multiple(() =>
            {
                Assert.That(result.ContainsKey("TestPrivateProperty"), Is.True);
                Assert.That(result["TestPrivateProperty"], Is.EqualTo(_fixture.PrivatePropertyValue));
            });
        }

        [Test]
        public void StaticPropertiesAreNotCaptured()
        {
            var result = _fixture.GetState();
            Assert.That(result.ContainsKey(nameof(GetStateTestClass.TestStaticProperty)), Is.False);
        }

        [Test]
        public void EnumerablePropertiesAreNotCaptured()
        {
            var result = _fixture.GetState();
            Assert.That(result.ContainsKey(nameof(GetStateTestClass.TestEnumerable)), Is.False);
        }

        [Test]
        public void ClassesAreNotCaptured()
        {
            var result = _fixture.GetState();
            Assert.That(result.ContainsKey(nameof(GetStateTestClass.TestObject)), Is.False);
        }

        [Test]
        public void AbstractsAreNotCaptured()
        {
            var result = _fixture.GetState();
            Assert.That(result.ContainsKey(nameof(GetStateTestClass.TestInterfaceImplementation)), Is.False);
        }


        private class WhenGettingStateTestFixture
        {
            private readonly CommitmentsV2.Services.StateService _stateService;
            public readonly GetStateTestClass TestObject;
            private Dictionary<string, object> _result;
            public readonly long PrivatePropertyValue;

            public WhenGettingStateTestFixture()
            {
                var autoFixture = new Fixture();
                _stateService = new CommitmentsV2.Services.StateService();
                PrivatePropertyValue = autoFixture.Create<long>();
                TestObject = new GetStateTestClass(PrivatePropertyValue);
            }

            public Dictionary<string, object> GetState()
            {
                _result = _stateService.GetState(TestObject);
                return _result;
            }
        }

        private class GetStateTestClass
        {
            public GetStateTestClass(long privatePropertyValue)
            {
                var autoFixture = new Fixture();
                TestPrivateProperty = privatePropertyValue;
                TestPublicProperty = autoFixture.Create<string>();
                TestObject = new GetStateTestSubClass();
                TestEnumerable = new List<GetStateTestSubClass>();
                TestInterfaceImplementation = new StateTestInterfaceImplementation();
            }

            public string TestPublicProperty { get; }
            private long TestPrivateProperty { get; }
            public GetStateTestSubClass TestObject { get; }
            public List<GetStateTestSubClass> TestEnumerable { get; }
            public static string TestStaticProperty { get; }
            public IStateTestInterface TestInterfaceImplementation { get; }
        }

        private class GetStateTestSubClass
        {
        }

        private interface IStateTestInterface
        {
        }

        private class StateTestInterfaceImplementation : IStateTestInterface
        {
        }
    }
}
