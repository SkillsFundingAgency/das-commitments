using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger<ValuesController> _logger;

        public ValuesController(ILogger<ValuesController> logger)
        {
            _logger = logger;
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            _logger.LogInformation("Testing Get Values");
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            var a = true;
            if(a == true)
                throw new ApiException(12,"Hello Error", new Exception("Inner A", new Exception("Inner B")));
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post(string value)
        {
            var a = true;
            if (a == true)
                throw new ApiException(1, "Hello Domain Error");
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
