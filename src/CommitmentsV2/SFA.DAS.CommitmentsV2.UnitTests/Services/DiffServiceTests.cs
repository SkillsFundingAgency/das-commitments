using System;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Services;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public class DiffServiceTests
    {
        private DiffServiceTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new DiffServiceTestsFixture();
        }

        [Test]
        public void IdenticalItemsProducesEmptyDiff()
        {
            _fixture.WithIdenticalItems().GenerateDiff();
            Assert.That(_fixture.Result, Is.Empty);
        }

        [Test]
        public void DifferentItemsAreReturned()
        {
            _fixture.WithRandomInitialItem().WithDifferentUpdatedValues().GenerateDiff();

            foreach (var item in _fixture.InitialItem)
            {
                var resultItem = _fixture.Result.Single(x => x.PropertyName == item.Key);
                Assert.Multiple(() =>
                {
                    Assert.That(resultItem.InitialValue, Is.EqualTo(item.Value));
                    Assert.That(resultItem.UpdatedValue, Is.EqualTo(_fixture.UpdatedItem[item.Key]));
                });
            }
        }

        [Test]
        public void ComparisonToNullInitialStateReturnsAllItemsInUpdated()
        {
            _fixture.WithNullInitialItem().WithRandomUpdatedItem().GenerateDiff();

            Assert.That(_fixture.Result, Has.Count.EqualTo(_fixture.UpdatedItem.Count));
            foreach (var item in _fixture.UpdatedItem)
            {
                var resultItem = _fixture.Result.Single(x => x.PropertyName == item.Key);
                Assert.Multiple(() =>
                {
                    Assert.That(resultItem.InitialValue, Is.Null);
                    Assert.That(resultItem.UpdatedValue, Is.EqualTo(item.Value));
                });
            }
        }

        [Test]
        public void ComparisonToInitialItemWithNullValuesReturnsAllItemsInUpdated()
        {
            _fixture.WithInitialItemsWithNullValues().WithDifferentUpdatedValues().GenerateDiff();

            Assert.That(_fixture.Result, Has.Count.EqualTo(_fixture.UpdatedItem.Count));
            foreach (var item in _fixture.UpdatedItem)
            {
                var resultItem = _fixture.Result.Single(x => x.PropertyName == item.Key);
                Assert.Multiple(() =>
                {
                    Assert.That(resultItem.InitialValue, Is.Null);
                    Assert.That(resultItem.UpdatedValue, Is.EqualTo(item.Value));
                });
            }
        }

        [Test]
        public void ComparisonToNullUpdatedStateReturnsAllItemsInInitial()
        {
            _fixture.WithRandomInitialItem().WithNullUpdatedItem().GenerateDiff();

            Assert.That(_fixture.Result, Has.Count.EqualTo(_fixture.InitialItem.Count));
            foreach (var item in _fixture.InitialItem)
            {
                var resultItem = _fixture.Result.Single(x => x.PropertyName == item.Key);
                Assert.Multiple(() =>
                {
                    Assert.That(resultItem.UpdatedValue, Is.Null);
                    Assert.That(resultItem.InitialValue, Is.EqualTo(item.Value));
                });
            }
        }

        private class DiffServiceTestsFixture
        {
            private readonly Fixture _autoFixture;
            private readonly DiffService _diffService;
            public IReadOnlyList<DiffItem> Result { get; private set; }
            public Dictionary<string, object> InitialItem;
            public Dictionary<string, object> UpdatedItem;

            public DiffServiceTestsFixture()
            {
                _autoFixture = new Fixture();
                _diffService = new DiffService();

                InitialItem = null;
                UpdatedItem = null;
            }

            public DiffServiceTestsFixture WithNullInitialItem()
            {
                InitialItem = null;
                return this;
            }

            public DiffServiceTestsFixture WithInitialItemsWithNullValues()
            {
                InitialItem = GenerateRandomDataWithNullValues();
                return this;
            }

            public DiffServiceTestsFixture WithNullUpdatedItem()
            {
                UpdatedItem = null;
                return this;
            }

            public DiffServiceTestsFixture WithRandomInitialItem()
            {
                InitialItem = GenerateRandomData();
                return this;
            }

            public DiffServiceTestsFixture WithRandomUpdatedItem()
            {
                UpdatedItem = GenerateRandomData();
                return this;
            }

            public DiffServiceTestsFixture WithDifferentUpdatedValues()
            {
                UpdatedItem = GenerateModifiedData(InitialItem);
                return this;
            }

            public DiffServiceTestsFixture WithIdenticalItems()
            {
                InitialItem = GenerateRandomData();
                UpdatedItem = TestHelper.Clone(InitialItem);
                return this;
            }

            public void GenerateDiff()
            {
                Result = _diffService.GenerateDiff(InitialItem, UpdatedItem);
            }

            private Dictionary<string, object> GenerateRandomData()
            {
                var result = new Dictionary<string, object>();
                for(var i=0; i<10; i++)
                {
                    result.Add(_autoFixture.Create<string>(), _autoFixture.Create<string>());
                    result.Add(_autoFixture.Create<string>(), _autoFixture.Create<long>());
                    result.Add(_autoFixture.Create<string>(), _autoFixture.Create<DateTime>());
                }
                return result;
            }

            private Dictionary<string, object> GenerateRandomDataWithNullValues()
            {
                var result = new Dictionary<string, object>();
                for (var i = 0; i < 10; i++)
                {
                    result.Add(_autoFixture.Create<string>(), null);
                }
                return result;
            }

            private Dictionary<string, object> GenerateModifiedData(Dictionary<string, object> source)
            {
                var result = new Dictionary<string, object>();

                foreach (var sourceItem in source)
                {
                    switch (sourceItem.Value)
                    {
                        case null:
                            result.Add(sourceItem.Key, "modified");
                            continue;
                        case string stringValue:
                            result.Add(sourceItem.Key, stringValue + "_modified");
                            continue;
                        case long longValue:
                            result.Add(sourceItem.Key, longValue+1);
                            continue;
                        case DateTime dateTimeValue:
                            result.Add(sourceItem.Key, dateTimeValue.AddDays(1));
                            continue;
                    }
                }

                return result;
            }
        }
    }
}
