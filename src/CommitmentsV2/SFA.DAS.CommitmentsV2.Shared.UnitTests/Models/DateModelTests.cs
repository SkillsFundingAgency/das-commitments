﻿using System;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Shared.Models;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.Models
{
    [TestFixture]
    public class DateModelTests
    {
        [Test]
        public void Constructor_WithoutDate_ShouldNotThrowException()
        {
            var dt = new DateModel();

            Assert.Pass("Completed without exception");
        }

        [Test]
        public void Constructor_WithDate_ShouldNotThrowException()
        {
            var dt = new DateModel(DateTime.Now);

            Assert.Pass("Completed without exception");
        }

        [Test]
        public void IsValid_ConstructedWithValidDate_ShouldBeTrue()
        {
            var dt = new DateModel(DateTime.Now);

            Assert.True(dt.IsValid);
        }

        [Test]
        public void IsValid_ConstructedWithoutDate_ShouldBeFalse()
        {
            var dt = new DateModel();

            Assert.False(dt.IsValid);
        }

        private const int MaxYearSupportedByDateTime = 9999;

        [TestCase(2019, 1, 1, true)]
        [TestCase(2019, 12, 31, true)]
        [TestCase(2019, 02, 29, false)]
        [TestCase(2019, 06, 31, false)]
        public void IsValid_WithSpecifiedYearMonthDay_ShouldSetIsValidCorrectly(int year, int month, int day, bool expectedIsValid)
        {
            // arrange
            var dt = new DateModel();

            dt.Year = year;
            dt.Month = month;
            dt.Day = day;

            // act
            var actualIsValid = dt.IsValid;

            // assert
            Assert.AreEqual(expectedIsValid, actualIsValid);
        }

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(32)]
        [TestCase(null)]
        public void Day_SetToInvalidValue_ShouldNotBeValid(int? day)
        {
            var dt = new DateModel();
            dt.Day = day;
            Assert.IsFalse(dt.IsValid);
        }

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(13)]
        [TestCase(null)]
        public void Month_SetToInvalidValue_ShouldNotBeValid(int? month)
        {
            var dt = new DateModel();

            dt.Month = month;
            Assert.IsFalse(dt.IsValid);
        }

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(null)]
        public void Year_SetToInvalidValue_ShouldNotBeValid(int? year)
        {
            var dt = new DateModel();
            dt.Year = year;
            Assert.IsFalse(dt.IsValid);
        }

    }
}
