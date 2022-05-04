using System.Threading.Tasks;
using System.Web.Http;


namespace SFA.DAS.Commitments.Support.SubSite.Controllers
{
    //public class SearchController : ApiController
    //{
    //    private readonly IAccountHandler _handler;

    //    public SearchController(IAccountHandler handler)
    //    {
    //        _handler = handler;
    //    }

    //    [HttpGet]
    //    [Route("api/search/accounts/{pagesize}/{pagenumber}")]
    //    public async Task<IHttpActionResult> Accounts(int pageSize, int pageNumber)
    //    {
    //        var accounts = await _handler.FindAllAccounts(pageSize, pageNumber);
    //        return Json(accounts);
    //    }

    //    [HttpGet]
    //    [Route("api/search/accounts/totalCount/{pageSize}")]
    //    public async Task<IHttpActionResult> AllAccountsTotalCount(int pageSize)
    //    {
    //        var accounts = await _handler.TotalAccountRecords(pageSize);
    //        return Json(accounts);
    //    }
    //}
}