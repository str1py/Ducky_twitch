using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ducky.Helpers;
using Ducky.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Ducky.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class LogController : Controller
    {
        LogService logService = Startup.GetLog();
        [HttpGet]
        [Produces("application/json")]
        public IEnumerable<LogMessageModel> Get()
        {
            return logService.GetLogList(); 
        }

        [HttpGet("{count}")]
        public IEnumerable<LogMessageModel> GetLog(int count)
        {
            return null;
        }

        [HttpGet("{type}")]
        public IEnumerable<LogMessageModel> GetLogByType(int type)
        {
            //return ONLY ERRORS STATS BEDUB etc.
            return null;
        }

    }
}