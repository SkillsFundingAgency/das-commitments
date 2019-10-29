using System.Collections.Generic;
using AutoFixture;
using NUnit.Framework;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.StateService
{
    [TestFixture]
    public class WhenGettingState
    {
        
        private WhenGettingStateTestFixture _fixture;
        
        [SetUp]
        public void Arrange()
        {
            _fixture = new WhenGettingStateTestFixture();
        }

        [Test]
        public void ThenPublicInstancePropertiesAreCaptured()
        {
            var result = _fixture.GetState();
            Assert.IsTrue(result.ContainsKey(nameof(GetStateTestClass.TestPublicProperty)));
            Assert.AreEqual(_fixture.TestObject.TestPublicProperty, result[nameof(GetStateTestClass.TestPublicProperty)]);
        }

        [Test]
        public void ThenPrivateInstancePropertiesAreCaptured()
        {
            var result = _fixture.GetState();
            Assert.IsTrue(result.ContainsKey("TestPrivateProperty"));
            Assert.AreEqual(_fixture.PrivatePropertyValue, result["TestPrivateProperty"]);
        }

        [Test]
        public void ThenStaticPropertiesAreNotCaptured()
        {
            var result = _fixture.GetState();
            Assert.IsFalse(result.ContainsKey(nameof(GetStateTestClass.TestStaticProperty)));
        }

        [Test]
        public void ThenEnumerablePropertiesAreNotCaptured()
        {
            var result = _fixture.GetState();
            Assert.IsFalse(result.ContainsKey(nameof(GetStateTestClass.TestEnumerable)));
        }

        [Test]
        public void ThenObjectsAreNotCaptured()
        {
            var result = _fixture.GetState();
            Assert.IsFalse(result.ContainsKey(nameof(GetStateTestClass.TestObject)));
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
            }

            public string TestPublicProperty { get; }
            private long TestPrivateProperty { get; }
            public GetStateTestSubClass TestObject { get; }
            public List<GetStateTestSubClass> TestEnumerable { get; }
            public static string TestStaticProperty { get; }
        }

        private class GetStateTestSubClass
        {
        }

    }
}
