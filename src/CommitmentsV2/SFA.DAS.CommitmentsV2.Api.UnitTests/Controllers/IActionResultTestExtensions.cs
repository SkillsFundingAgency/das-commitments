﻿using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers
{
    internal static class IActionResultTestExtensions
    {
        public static ObjectResult VerifyReturnsModel(this IActionResult result)
        {
            return result.VerifyResponseObjectType<ObjectResult>();
        }

        public static TExpectedResponseType VerifyResponseObjectType<TExpectedResponseType>(this IActionResult result) where TExpectedResponseType : IActionResult
        {
            Assert.IsTrue(result is TExpectedResponseType, $"Expected response type {typeof(TExpectedResponseType)} but got {result.GetType()}");
            return (TExpectedResponseType)result;
        }

        public static TExpectedModel WithModel<TExpectedModel>(this ObjectResult result) where TExpectedModel : class
        {
            Assert.IsInstanceOf<TExpectedModel>(result.Value);
            return result.Value as TExpectedModel;
        }
    }
}