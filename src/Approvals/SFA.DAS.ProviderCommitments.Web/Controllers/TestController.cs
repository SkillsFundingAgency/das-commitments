using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SFA.DAS.ProviderCommitments.Data;
using SFA.DAS.ProviderCommitments.Web.Models;

namespace SFA.DAS.ProviderCommitments.Web.Controllers
{
    public class TestController : Controller
    {
        private readonly ProviderDbContext _dbContext;

        public TestController(ProviderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ActionResult HitDb()
        {
            var testModel = new TestModel
            {
                Draft = _dbContext.DraftApprenticeships.Count(),
                Approved = _dbContext.ConfirmedApprenticeships.Count()
            };

            return View(testModel);
        }
    }
}