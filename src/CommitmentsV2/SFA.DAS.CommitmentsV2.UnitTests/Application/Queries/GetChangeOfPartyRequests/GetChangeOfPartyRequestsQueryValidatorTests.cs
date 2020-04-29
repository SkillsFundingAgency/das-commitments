﻿using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation.Results;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedProviders;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfPartyRequests;
using SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetProvider;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetChangeOfPartyRequests
{
    [TestFixture]
    [Parallelizable]
    public class GetChangeOfPartyRequestsQueryValidatorTests
    {
        private GetChangeOfPartyRequestsQueryValidatorTestsFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new GetChangeOfPartyRequestsQueryValidatorTestsFixture();
        }

        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void Validate_WhenValidating_ThenShouldValidate(int accountId, bool isValid)
        {
            var validationResult = _fixture.Validate(accountId);

            Assert.AreEqual(isValid, validationResult.IsValid);
        }
    }

    public class GetChangeOfPartyRequestsQueryValidatorTestsFixture
    {
        public GetChangeOfPartyRequestsQueryValidator Validator { get; set; }

        public GetChangeOfPartyRequestsQueryValidatorTestsFixture()
        {
            Validator = new GetChangeOfPartyRequestsQueryValidator();
        }

        public ValidationResult Validate(long apprenticeshipId)
        {
            return Validator.Validate(new GetChangeOfPartyRequestsQuery(apprenticeshipId));
        }
    }
}
