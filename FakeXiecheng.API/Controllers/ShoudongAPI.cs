using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FakeXiecheng.API.Controllers
{
    [Route("api/shoudongapi")]
    //[ApiController]
    public class ShoudongAPIController
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "my value1", "my value2" };
        }
    }
}
