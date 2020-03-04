using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SFA.DAS.CommitmentsV2.Shared.ActionResults;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.ActionResults
{
    public class FileCallbackResultTests
    {
        [Test]
        public void Then_If_The_Media_Type_Is_Not_Correct_An_Exception_Is_Thrown()
        {
            Assert.Throws<FormatException>(() => new FileCallbackResult("wrong", delegate{ return null; }));
        }

        [Test]
        public void Then_If_There_Is_No_Callback_An_Exception_Is_Thrown()
        {
            Assert.Throws<ArgumentNullException>(() => new FileCallbackResult("text/csv", null));
        }
    }
}
